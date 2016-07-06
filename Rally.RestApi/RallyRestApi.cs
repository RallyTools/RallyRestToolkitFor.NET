using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Rally.RestApi.Response;
using System.Text.RegularExpressions;
using System.Threading;
using Rally.RestApi.Web;
using Rally.RestApi.Connection;
using Rally.RestApi.Json;
using Rally.RestApi.Auth;
using Rally.RestApi.Exceptions;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Rally.RestApi
{
	/// <summary>
	/// The main interface to the Rally REST API
	/// </summary>
	public class RallyRestApi
	{
		/// <summary>
		/// The endpoint for getting a CSRF token. This has custom logic in HttpService for SSO users.
		/// </summary>
		public const string SECURITY_ENDPOINT = "/security/authorize.js";
		TraceSource traceSource = new TraceSource("RallyRestApi");

		#region Enumeration: AuthenticationResult
		/// <summary>
		/// Enumeration of the different authentication results that may occur.
		/// </summary>
		public enum AuthenticationResult
		{
			/// <summary>
			/// The user is not authorized.
			/// </summary>
			NotAuthorized = 1, // Must be default entry as a protection for code checking the authenticated property.
			/// <summary>
			/// The user needs to perform SSO authentication.
			/// </summary>
			PendingSSO = 2,
			/// <summary>
			/// The user is authenticated.
			/// </summary>
			Authenticated = 3,
		}
		#endregion

		#region Enumeration: HeaderType
		/// <summary>
		/// Enumeration of the valid HTTP headers that
		/// may be passed on REST requests
		/// </summary>
		public enum HeaderType
		{
			/// <summary>
			/// X-RallyIntegrationOperation
			/// </summary>
			[StringValue("X-RallyIntegrationOperation")]
			Operation,
			/// <summary>
			/// X-Trace-Id
			/// </summary>
			[StringValue("X-Trace-Id")]
			Guid,
			/// <summary>
			/// X-RallyIntegrationLibrary
			/// </summary>
			[StringValue("X-RallyIntegrationLibrary")]
			Library,
			/// <summary>
			/// X-RallyIntegrationName
			/// </summary>
			[StringValue("X-RallyIntegrationName")]
			Name,
			/// <summary>
			/// X-RallyIntegrationVendor
			/// </summary>
			[StringValue("X-RallyIntegrationVendor")]
			Vendor,
			/// <summary>
			/// X-RallyIntegrationVersion
			/// </summary>
			[StringValue("X-RallyIntegrationVersion")]
			Version
		}
		#endregion

		#region Child Class: StringValue (Attribute)
		internal class StringValue : Attribute
		{
			private string _value;

			public StringValue(string value) { _value = value; }

			public string Value { get { return _value; } }
		}
		#endregion

		#region Constants
		/// <summary>
		/// The identifier for the authentication cookie used by Rally.
		/// </summary>
		public const string ZSessionID = "ZSESSIONID";
		/// <summary>
		/// The maximum number of threads allowed when performing parallel operations.
		/// </summary>
		private const int MAX_THREADS_ALLOWED = 6;
		/// <summary>
		/// The default WSAPI version to use
		/// </summary>
		public const string DEFAULT_WSAPI_VERSION = "v2.0";
		/// <summary>
		/// The default Trace fields
		/// </summary>
		public const TraceFieldEnum DEFAULT_TRACE_FIELDS = TraceFieldEnum.Data | TraceFieldEnum.Headers | TraceFieldEnum.Cookies;
		/// <summary>
		/// The default server to use: (https://rally1.rallydev.com)
		/// </summary>
		public const string DEFAULT_SERVER = "https://rally1.rallydev.com";
		/// <summary>
		/// /// The default auth arror
		/// </summary>
		public const string AUTH_ERROR = "You must authenticate against CA Agile Central prior to performing any data operations.";
		#endregion

		#region Properties and Fields
		private ApiAuthManager authManger;
		private HttpService httpService;
		private int maxRetries;
		private readonly DynamicJsonSerializer serializer = new DynamicJsonSerializer();
		/// <summary>
		/// The HTTP headers to be included on all REST requests
		/// </summary>
		public Dictionary<HeaderType, string> Headers { get; private set; }
		/// <summary>
		/// The state of authentication for this API instance.
		/// </summary>
		public AuthenticationResult AuthenticationState { get; private set; }
		/// <summary>
		/// The connection info thsi API is using.
		/// </summary>
		public ConnectionInfo ConnectionInfo { get; private set; }
		/// <summary>
		/// The WSAPI version we are talking to.
		/// </summary>
		public virtual String WsapiVersion { get; set; }
		/// <summary>
		/// Is this connection using WSAPI 2?
		/// </summary>
		internal bool IsWsapi2 { get { return !new Regex("^1[.]\\d+").IsMatch(WsapiVersion); } }
		#endregion

		#region Calculated Properties

		#region WebServiceUrl
		/// <summary>
		/// The full WSAPI url
		/// </summary>
		public string WebServiceUrl
		{
			get { return String.Format("{0}slm/webservice/{1}", httpService.Server.AbsoluteUri, WsapiVersion); }
		}
		#endregion

		#endregion

		#region Constructor
		/// <summary>
		/// Construct a new RallyRestApi configured to work with the specified WSAPI version
		/// </summary>
		/// <param name="authManger">The authorization manager to use when authentication requires it. If no driver is 
		/// provided a console authentication manager will be used which does not allow SSO authentication.</param>
		/// <param name="webServiceVersion">The WSAPI version to use (defaults to DEFAULT_WSAPI_VERSION)</param>
		/// <param name="maxRetries">Requests will be attempted a number of times (defaults to 3)</param>
		/// <param name="traceInfo">Controls diagnostic trace information being logged</param>
		/// <example>
		/// For a console application, no authentication manager is needed as shown in this example.
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// </code>
		/// For UI applications, an authentication manager must be provided. The authentication providers will 
		/// configure the API and create the linkages as part of the constructor. Please see the documentation for 
		/// the RestApiAuthMgrWpf and RestApiAuthMgrWinforms for more information.
		/// <code language="C#">
		/// // You must define your own private application token. This ensures that your login details are not overwritten by someone else.
		/// string applicationToken = "RallyRestAPISample";
		/// // You must set a user specific salt for encryption.
		/// string encryptionKey = "UserSpecificSaltForEncryption";
		/// // You must define your own encryption routines.
		/// IEncryptionRoutines encryptionUtilities = new EncryptionUtilities();
		/// 
		/// // Instantiate authorization manager
		/// wpfAuthMgr = new RestApiAuthMgrWpf(applicationToken, encryptionKey, encryptionUtilities);
		/// </code>
		/// </example>
		public RallyRestApi(ApiAuthManager authManger = null, string webServiceVersion = DEFAULT_WSAPI_VERSION, int maxRetries = 3, TraceFieldEnum traceInfo = RallyRestApi.DEFAULT_TRACE_FIELDS)
		{
			// NOTE: The example for using the RestApiAuthMgrWpf is also shown there. Make sure you 
			// update both if you change it.

			TraceHelper.TraceFields = traceInfo;

			if (authManger == null)
				authManger = new ApiConsoleAuthManager(webServiceVersion, traceInfo);

			this.authManger = authManger;

			WsapiVersion = webServiceVersion;
			if (String.IsNullOrWhiteSpace(WsapiVersion))
				WsapiVersion = DEFAULT_WSAPI_VERSION;

			AuthenticationState = AuthenticationResult.NotAuthorized;

			this.maxRetries = maxRetries;
		}
		#endregion

		#region Authenticate
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="userName">The user name to be used for access</param>
		/// <param name="zSessionID">The ZSessionID to be used for access. This would have been provided by Rally on a previous call.</param>
		/// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
		/// <returns>An <see cref="AuthenticationResult"/> that indicates the current state of the authentication process.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// WebProxy myProxy = new WebProxy();
		/// restApi.AuthenticateWithZSessionID("myuser@company.com", "zSessionID", proxy: myProxy);
		/// </code>
		/// </example>
		public AuthenticationResult AuthenticateWithZSessionID(string userName, string zSessionID,
			string rallyServer = DEFAULT_SERVER, WebProxy proxy = null, bool allowSSO = true)
		{
			if (String.IsNullOrWhiteSpace(rallyServer))
				rallyServer = DEFAULT_SERVER;

			if (!authManger.IsUiSupported)
				throw new InvalidOperationException("ZSessionID authentication is only supported with a valid SSO provider.");

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.ZSessionID;
			connectionInfo.UserName = userName;
			connectionInfo.ZSessionID = zSessionID;
			connectionInfo.Server = new Uri(rallyServer);
			connectionInfo.Proxy = proxy;
			connectionInfo.IdpServer = ConnectionInfo.IdpServer;
			return AuthenticateWithConnectionInfo(connectionInfo, allowSSO);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="apiKey">The API key to be used for access</param>
		/// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <returns>An <see cref="AuthenticationResult"/> that indicates the current state of the authentication process.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// WebProxy myProxy = new WebProxy();
		/// restApi.AuthenticateWithApiKey("ApiKeyFromRally", proxy: myProxy);
		/// </code>
		/// </example>
		public AuthenticationResult AuthenticateWithApiKey(string apiKey,
			string rallyServer = DEFAULT_SERVER, WebProxy proxy = null)
		{
			if (String.IsNullOrWhiteSpace(rallyServer))
				rallyServer = DEFAULT_SERVER;


			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.ApiKey;
			connectionInfo.ApiKey = apiKey;
			connectionInfo.Server = new Uri(rallyServer);
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, false);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="apiKey">The API key to be used for access</param>
		/// <param name="serverUrl">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <returns>The current state of the authentication process. <see cref="AuthenticationResult"/></returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// WebProxy myProxy = new WebProxy();
		/// restApi.AuthenticateWithApiKey("ApiKeyFromRally", new Uri("https://myserverurl"), proxy: myProxy);
		/// </code>
		/// </example>
		public AuthenticationResult AuthenticateWithApiKey(string apiKey, Uri serverUrl, WebProxy proxy = null)
		{
			if (serverUrl == null)
				serverUrl = new Uri(DEFAULT_SERVER);

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.ApiKey;
			connectionInfo.ApiKey = apiKey;
			connectionInfo.Server = serverUrl;
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, false);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="username">The user name to be used for access</param>
		/// <param name="password">The password to be used for access</param>
		/// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
		/// <returns>The current state of the authentication process. <see cref="AuthenticationResult"/></returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// WebProxy myProxy = new WebProxy();
		/// restApi.Authenticate("myuser@company.com", "password", proxy: myProxy);
		/// </code>
		/// </example>
		public AuthenticationResult Authenticate(string username, string password, string rallyServer = DEFAULT_SERVER, WebProxy proxy = null, bool allowSSO = true)
		{
			if (String.IsNullOrWhiteSpace(rallyServer))
				rallyServer = DEFAULT_SERVER;

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.Basic;
			connectionInfo.UserName = username;
			connectionInfo.Password = password;
			connectionInfo.Server = new Uri(rallyServer);
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, allowSSO);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="username">The user name to be used for access</param>
		/// <param name="password">The password to be used for access</param>
		/// <param name="serverUrl">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
		/// <returns>The current state of the authentication process. <see cref="AuthenticationResult"/></returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// WebProxy myProxy = new WebProxy();
		/// restApi.Authenticate("myuser@company.com", "password", new Uri("https://myserverurl"), proxy: myProxy);
		/// </code>
		/// </example>
		public AuthenticationResult Authenticate(string username, string password, Uri serverUrl, WebProxy proxy = null, bool allowSSO = true)
		{
			if (serverUrl == null)
				serverUrl = new Uri(DEFAULT_SERVER);

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.Basic;
			connectionInfo.UserName = username;
			connectionInfo.Password = password;
			connectionInfo.Server = serverUrl;
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, allowSSO);
		}

		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		private AuthenticationResult AuthenticateWithConnectionInfo(ConnectionInfo connectionInfo, bool allowSSO)
		{
			string exceptionMessage = "";
			AuthenticationResult authWithConnection = AuthenticateWithConnectionInfoBaseMethod(connectionInfo, allowSSO, out exceptionMessage);
			return authWithConnection;
		}
		#endregion

		private AuthenticationResult AuthenticateWithConnectionInfoBaseMethod(ConnectionInfo connectionInfo, bool allowSSO,
				out string exceptionMessage)
		{
			exceptionMessage = "";

			this.ConnectionInfo = connectionInfo;
			httpService = new HttpService(authManger, connectionInfo);

			Headers = new Dictionary<HeaderType, string>();
			Assembly assembly = typeof(RallyRestApi).Assembly;
			Headers.Add(HeaderType.Library, String.Format("{0} v{1}",
					((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly,
					typeof(AssemblyTitleAttribute), false)).Title, assembly.GetName().Version.ToString()));
			Headers.Add(HeaderType.Vendor,
					((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly,
					typeof(AssemblyCompanyAttribute), false)).Company);
			Headers.Add(HeaderType.Name, ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly,
					typeof(AssemblyTitleAttribute), false)).Title);
			Headers.Add(HeaderType.Version, assembly.GetName().Version.ToString());
			Headers.Add(HeaderType.Guid, Guid.NewGuid().ToString());

			try
			{
				dynamic userObject = GetCurrentUser("UserName");
				ConnectionInfo.UserName = userObject["UserName"];
				AuthenticationState = AuthenticationResult.Authenticated;
			}
			catch (Exception e)
			{
				if ((e is WebException) && (((WebException)e).Status == WebExceptionStatus.ConnectFailure))
				{
					throw;
				}

				if ((allowSSO) && (!httpService.PerformSsoAuthentication()))
				{
					Logout();
					throw;
				}

				AuthenticationState = AuthenticationResult.NotAuthorized;
				exceptionMessage = e.Message;
			}

			return AuthenticationState;
		}

		#region CreateIdpAuthentication
		/// <summary>
		/// Configures authentication to run against an IDP.
		/// </summary>
		internal void CreateIdpAuthentication(Uri idpServer, WebProxy proxy = null)
		{
			if (idpServer == null)
				throw new ArgumentNullException("idpServer");

			ConnectionInfo = new ConnectionInfo();
			ConnectionInfo.AuthType = AuthorizationType.Basic;
			ConnectionInfo.UserName = String.Empty;
			ConnectionInfo.Password = String.Empty;
			ConnectionInfo.IdpServer = idpServer;
			ConnectionInfo.Proxy = proxy;
		}
		#endregion

		#region Logout
		/// <summary>
		/// Logs this API out from any connection to Rally and clears the authentication configuration.
		/// </summary>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi restApi = new RallyRestApi();
		/// WebProxy myProxy = new WebProxy();
		/// restApi.AuthenticateWithZSessionID("myuser@company.com", "zSessionID", proxy: myProxy);
		/// 
		/// restApi.Logout();
		/// </code>
		/// </example>
		public void Logout()
		{
			AuthenticationState = AuthenticationResult.NotAuthorized;
			ConnectionInfo = null;
			httpService = null;
		}
		#endregion

		#region SetDefaultConnectionLimit
		/// <summary>
		/// Sets the default maximum concurrent connection limit for this application.
		/// <note>This will affect all connections that use Service Point.</note>
		/// </summary>
		/// <param name="maxConnections">The maximum number of concurrent connections. Allowed values are between 1 and 25.</param>
		/// <example>
		/// <code language="C#">
		/// RallyRestApi.SetDefaultConnectionLimit(10);
		/// </code>
		/// </example>
		public static void SetDefaultConnectionLimit(ushort maxConnections)
		{
			if ((maxConnections < 1) || (25 < maxConnections))
				throw new ArgumentOutOfRangeException("maxConnections", "Allowed values are between 1 and 25.");

			ServicePointManager.DefaultConnectionLimit = maxConnections;
		}
		#endregion

		#region Post
		/// <summary>
		/// Performs a post of data to the provided URI.
		/// </summary>
		/// <param name="relativeUri">The relative URI to post the data to.</param>
		/// <param name="data">The data to submit to Rally.</param>
		/// <returns>A <see cref="DynamicJsonObject"/> with information on the response from Rally.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// DynamicJsonObject data = objectToPost;
		/// restApi.Post("defect/12345", data)
		/// </code>
		/// </example>
		public DynamicJsonObject Post(String relativeUri, DynamicJsonObject data)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			Uri uri = new Uri(String.Format("{0}slm/webservice/{1}/{2}", httpService.Server.AbsoluteUri, WsapiVersion, relativeUri));
			string postData = serializer.Serialize(data);
			return serializer.Deserialize(httpService.Post(uri, postData, GetProcessedHeaders()));
		}
		#endregion

		#region DoDelete
		/// <summary>
		/// Performs a delete action.
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		private DynamicJsonObject DoDelete(Uri uri, bool retry = true)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			var response = serializer.Deserialize(httpService.Delete(GetSecuredUri(uri), GetProcessedHeaders()));
			if (retry && ConnectionInfo.SecurityToken != null && response[response.Fields.First()].Errors.Count > 0)
			{
				ConnectionInfo.SecurityToken = null;
				return DoDelete(uri, false);
			}
			return response;
		}
		#endregion

		#region Query
		/// <summary>
		/// Perform a read against the WSAPI operation based
		/// on the data in the specified request
		/// </summary>
		/// <param name="request">The request configuration</param>
		/// <returns>A <see cref="DynamicJsonObject"/> with the response from Rally.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// // Build request
		/// Request request = new Request("defect");
		/// request.Fetch = new List&lt;string&gt;() { "Name", "Description", "FormattedID" };
		/// 
		/// request.Query = new Query("Name", Query.Operator.Equals, "My Defect").And(new Query("State", Query.Operator.Equals, "Submitted"));
		/// 
		/// // Make request and process results 
		/// QueryResult queryResult = restApi.Query(request);
		/// foreach (var result in queryResult.Results)
		/// {
		/// 	string itemName = result["Name"];
		/// }
		/// </code>
		/// </example>
		public QueryResult Query(Request request)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			DynamicJsonObject response;
			if (IsWsapi2)
				response = DoGetAsPost(request);
			else
				response = DoGet(GetFullyQualifiedUri(request.RequestUrl));

			var result = new QueryResult(response["QueryResult"]);
			int maxResultsAllowed = Math.Min(request.Limit, result.TotalResultCount);
			int alreadyDownloadedItems = request.Start - 1 + request.PageSize;
			var subsequentQueries = new List<Request>();

			while ((maxResultsAllowed - alreadyDownloadedItems) > 0)
			{
				Request newRequest = request.Clone();
				newRequest.Start = request.Start + request.PageSize;
				request.Start += request.PageSize;

				//makes sure partial pages are downloaded. IE limit 201
				newRequest.PageSize = Math.Min((maxResultsAllowed - alreadyDownloadedItems), request.PageSize);
				subsequentQueries.Add(newRequest);

				// Start has 1 for its lowest value.
				alreadyDownloadedItems = request.Start - 1 + request.PageSize;
			}

			TraceHelper.TraceMessage("The number of threaded requests is : {0}", subsequentQueries.Count);

			var resultDictionary = new Dictionary<int, QueryResult>();
			Parallel.ForEach(subsequentQueries, new ParallelOptions { MaxDegreeOfParallelism = MAX_THREADS_ALLOWED }, request1 =>
			{
				var response1 = DoGet(GetFullyQualifiedUri(request1.RequestUrl));
				lock (resultDictionary)
				{
					resultDictionary[request1.Start] = new QueryResult(response1["QueryResult"]);
				}
			});

			var allResults = new List<object>(result.Results);
			foreach (var sortedResult in resultDictionary.ToList()
					.OrderBy(p => p.Key))
			{
				result.Errors.AddRange(sortedResult.Value.Errors);
				result.Warnings.AddRange(sortedResult.Value.Warnings);
				allResults.AddRange(sortedResult.Value.Results);
			}

			result.Results = allResults;
			return result;
		}
		#endregion

		#region GetCurrentUser
		/// <summary>
		/// Get the current user
		/// </summary>
		/// <param name="fetchedFields">The fields that should be retrieved for the user.
		/// If no fields are specified, a * wild card will be used.</param>
		/// <returns>A <see cref="DynamicJsonObject"/> that contains the currently logged in user.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// DynamicJsonObject user = restApi.GetCurrentUser("Name", "FormattedID");
		/// string user = user["Name"];
		/// string userID = user["FormattedID"];
		/// </code>
		/// </example>
		public dynamic GetCurrentUser(params string[] fetchedFields)
		{
			return GetByReference("/user.js", fetchedFields);
		}
		#endregion

		#region GetSubscription
		/// <summary>
		/// Get the current subscription
		/// </summary>
		/// <param name="fetchedFields">An optional list of fields to be fetched</param>
		/// <returns>A <see cref="DynamicJsonObject"/> that contains the currently logged in user.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// DynamicJsonObject subscription = restApi.GetSubscription("Name", "FormattedID");
		/// string subscriptionName = subscription["Name"];
		/// string subscriptionFormattedID = subscription["FormattedID"];
		/// </code>
		/// </example>
		public dynamic GetSubscription(params string[] fetchedFields)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			return GetByReference("/subscription.js", fetchedFields);
		}
		#endregion

		#region GetByReference
		/// <summary>
		/// Get the object described by the specified type and object id.
		/// </summary>
		/// <param name="typePath">the type</param>
		/// <param name="oid">the object id</param>
		/// <param name="fetchedFields">the list of object fields to be fetched</param>
		/// <returns>A <see cref="DynamicJsonObject"/> containing the requested object.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <exception cref="InvalidOperationException">Occurs if authentication is not completed prior to calling this method.</exception>
		/// <example>
		/// <code language="C#">
		/// DynamicJsonObject item = restApi.GetByReference("defect", 12345, "Name", "FormattedID");
		/// string itemName = item["Name"];
		/// string itemFormattedID = item["FormattedID"];
		/// </code>
		/// </example>
		public dynamic GetByReference(string typePath, long oid, params string[] fetchedFields)
		{
			return GetByReference(string.Format("/{0}/{1}", typePath, oid), fetchedFields);
		}

		/// <summary>
		/// Get the object described by the specified reference.
		/// </summary>
		/// <param name="aRef">the reference</param>
		/// <param name="fetchedFields">the list of object fields to be fetched</param>
		/// <returns>A <see cref="DynamicJsonObject"/> containing the requested object.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <exception cref="InvalidOperationException">Occurs if authentication is not completed prior to calling this method.</exception>
		/// <exception cref="ArgumentNullException">Occurs if the passed in aRef parameter is null.</exception>
		/// <example>
		/// <code language="C#">
		/// string aRef = "https://preview.rallydev.com/slm/webservice/v2.0/defect/12345.js"
		/// DynamicJsonObject item = restApi.GetByReference(aRef, "Name", "FormattedID");
		/// string itemName = item["Name"];
		/// </code>
		/// </example>
		public dynamic GetByReference(string aRef, params string[] fetchedFields)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			if (aRef == null)
				throw new ArgumentNullException("aRef", "You must provide a reference to retrieve data from CA Agile Central.");

			if (fetchedFields.Length == 0)
			{
				fetchedFields = new string[] { "true" };
			}

			if (!aRef.Contains(".js"))
			{
				aRef = aRef + ".js";
			}

			DynamicJsonObject wrappedReponse = DoGet(GetFullyQualifiedUri(aRef + "?fetch=" + string.Join(",", fetchedFields)));
			return string.Equals(wrappedReponse.Fields.FirstOrDefault(), "OperationResult", StringComparison.CurrentCultureIgnoreCase) ? null : wrappedReponse[wrappedReponse.Fields.First()];
		}
		#endregion

		#region GetByReferenceAndWorkspace
		/// <summary>
		/// Get the object described by the specified reference scoped to the provided workspace.
		/// </summary>
		/// <param name="aRef">the reference</param>
		/// <param name="workspaceRef">workspace scope</param>
		/// <param name="fetchedFields">the list of object fields to be fetched</param>
		/// <returns>A <see cref="DynamicJsonObject"/> containing the requested object.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string aRef = "https://preview.rallydev.com/slm/webservice/v2.0/defect/12345.js"
		/// string workspaceRef = "/workspace/12345678910";
		/// DynamicJsonObject item = restApi.GetByReference(aRef, workspaceRef, "Name", "FormattedID");
		/// string itemName = item["Name"];
		/// </code>
		/// </example>
		public dynamic GetByReferenceAndWorkspace(string aRef, string workspaceRef, params string[] fetchedFields)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			if (fetchedFields.Length == 0)
			{
				fetchedFields = new string[] { "true" };
			}

			if (!aRef.Contains(".js"))
			{
				aRef = aRef + ".js";
			}

			string workspaceClause = "";
			if (workspaceRef != null)
				workspaceClause = "workspace=" + workspaceRef + "&";

			DynamicJsonObject wrappedReponse = DoGet(GetFullyQualifiedUri(aRef + "?" + workspaceClause + "fetch=" + string.Join(",", fetchedFields)));
			return string.Equals(wrappedReponse.Fields.FirstOrDefault(), "OperationResult", StringComparison.CurrentCultureIgnoreCase) ? null : wrappedReponse[wrappedReponse.Fields.First()];
		}
		#endregion

		#region Delete
		/// <summary>
		/// Delete the object described by the specified type and object id.
		/// </summary>
		/// <param name="workspaceRef">the workspace from which the object will be deleted.  Null means that the server will pick a workspace.</param>
		/// <param name="typePath">the type</param>
		/// <param name="oid">the object id</param>
		/// <returns>An OperationResult with information on the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string workspaceRef = "/workspace/12345678910";
		/// long objectID = 12345678912L;
		/// OperationResult deleteResult = restApi.Delete(workspaceRef, "Defect", objectID);
		///</code>
		///</example>
		public OperationResult Delete(string workspaceRef, string typePath, long oid)
		{
			return Delete(workspaceRef, string.Format("/{0}/{1}", typePath, oid));
		}

		/// <summary>
		/// Delete the object described by the specified type and object id.
		/// </summary>
		/// <param name="typePath">the type</param>
		/// <param name="oid">the object id</param>
		/// <returns>An OperationResult with information on the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// long objectID = 12345678912L;
		/// OperationResult deleteResult = restApi.Delete("Defect", objectID);
		/// </code>
		/// </example>
		public OperationResult Delete(string typePath, long oid)
		{
			return Delete(null, string.Format("/{0}/{1}", typePath, oid));
		}

		/// <summary>
		/// Delete the object described by the specified reference.
		/// </summary>
		/// <param name="aRef">the reference</param>
		/// <returns>An OperationResult with information on the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string objectRef = "/defect/12345678912";
		/// OperationResult deleteResult = restApi.Delete(objectRef);
		///</code>
		///</example>
		public OperationResult Delete(string aRef)
		{
			return Delete(null, aRef);
		}

		/// <summary>
		/// Delete the object described by the specified reference.
		/// </summary>
		/// <param name="workspaceRef">the workspace from which the object will be deleted.  Null means that the server will pick a workspace.</param>
		/// <param name="aRef">the reference</param>
		/// <returns>An OperationResult with information on the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string workspaceRef = "/workspace/12345678910";;
		/// string objectRef = "/defect/12345678912";
		/// OperationResult deleteResult = restApi.Delete(workspaceRef, objectRef);
		///</code>
		///</example>
		public OperationResult Delete(string workspaceRef, string aRef)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			var result = new OperationResult();
			if (!aRef.Contains(".js"))
			{
				aRef = aRef + ".js";
			}
			String workspaceClause = workspaceRef == null ? "" : "?workspace=" + workspaceRef;
			dynamic response = DoDelete(GetFullyQualifiedUri(aRef + workspaceClause));
			result.Errors.AddRange(DecodeArrayList(response.OperationResult.Errors));
			result.Warnings.AddRange(DecodeArrayList(response.OperationResult.Warnings));
			return result;
		}
		#endregion

		#region Create
		/// <summary>
		/// Create an object of the specified type from the specified object
		/// </summary>
		/// <param name="typePath">the type to be created</param>
		/// <param name="obj">the object to be created</param>
		/// <returns>A <see cref="CreateResult"/> with information on the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// DynamicJsonObject toCreate = new DynamicJsonObject();
		/// toCreate["Name"] = "My Defect";
		/// CreateResult createResult = restApi.Create("defect", toCreate);
		/// </code>
		/// </example>
		public CreateResult Create(string typePath, DynamicJsonObject obj)
		{
			return Create(null, typePath, obj);
		}

		/// <summary>
		/// Create an object of the specified type from the specified object
		/// </summary>
		/// <param name="workspaceRef">the workspace into which the object should be created.  Null means that the server will pick a workspace.</param>
		/// <param name="typePath">the type to be created</param>
		/// <param name="obj">the object to be created</param>
		/// <returns>A <see cref="CreateResult"/> with information on the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string workspaceRef = "/workspace/12345678910";
		/// DynamicJsonObject toCreate = new DynamicJsonObject();
		/// toCreate["Name"] = "My Defect";
		/// CreateResult createResult = restApi.Create(workspaceRef, "defect", toCreate);
		/// </code>
		/// </example>
		public CreateResult Create(string workspaceRef, string typePath, DynamicJsonObject obj)
        {
            NameValueCollection parameters = new NameValueCollection();
            if (workspaceRef != null)
            {
                parameters["workspace"] = workspaceRef;
            }
            return Create(typePath, obj, parameters);
        }

        /// <summary>
        /// Create an object of the specified type from the specified object
        /// </summary>
        /// <param name="typePath">the type to be created</param>
        /// <param name="obj">the object to be created</param>
        /// <param name="parameters">additional parameters to include in the create request</param>
        /// <returns>A <see cref="CreateResult"/> with information on the status of the request</returns>
        /// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
        /// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
        /// <example>
        /// <code language="C#">
        /// string workspaceRef = "/workspace/12345678910";
        /// DynamicJsonObject toCreate = new DynamicJsonObject();
        /// toCreate["Name"] = "My Defect";
        /// NameValueCollection parameters = new NameValueCollection();
        /// parameters["rankAbove"] = "/defect/12345";
        /// CreateResult createResult = restApi.Create("defect", toCreate, parameters);
        /// </code>
        /// </example>
        public CreateResult Create(string typePath, DynamicJsonObject obj, NameValueCollection parameters)
        {
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			var data = new DynamicJsonObject();
			data[typePath] = obj;
			DynamicJsonObject response = DoPost(FormatCreateUri(typePath, parameters), data);
			DynamicJsonObject createResult = response["CreateResult"];
			var createResponse = new CreateResult();
			if (createResult.HasMember("Object"))
			{
				createResponse.Object = createResult["Object"];
				createResponse.Reference = createResponse.Object["_ref"] as string;
			}
			createResponse.Errors.AddRange(DecodeArrayList(createResult["Errors"]));
			createResponse.Warnings.AddRange(DecodeArrayList(createResult["Warnings"]));
			return createResponse;
		}
		#endregion

		#region Update
		/// <summary>
		/// Update the item described by the specified reference with
		/// the fields of the specified object
		/// </summary>
		/// <param name="reference">the reference to be updated</param>
		/// <param name="obj">the object fields to update</param>
		/// <returns>An <see cref="OperationResult"/> describing the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string rallyRef = "https://preview.rallydev.com/slm/webservice/1.40/defect/12345.js";
		/// DynamicJsonObject toUpdate = new DynamicJsonObject(); 
		/// toUpdate["Description"] = "This is my defect."; 
		/// OperationResult updateResult = restApi.Update(rallyRef, toUpdate);
		/// </code>
		/// </example>
		public OperationResult Update(string reference, DynamicJsonObject obj)
		{
			return Update(Ref.GetTypeFromRef(reference), Ref.GetOidFromRef(reference), obj);
		}

        /// <summary>
		/// Update the item described by the specified reference with
		/// the fields of the specified object
		/// </summary>
		/// <param name="reference">the reference to be updated</param>
		/// <param name="obj">the object fields to update</param>
        /// <param name="parameters">additional query string parameters to be included on the request</param>
		/// <returns>An <see cref="OperationResult"/> describing the status of the request</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// string rallyRef = "https://preview.rallydev.com/slm/webservice/1.40/defect/12345.js";
		/// DynamicJsonObject toUpdate = new DynamicJsonObject(); 
		/// toUpdate["Description"] = "This is my defect."; 
        /// NameValueCollection parameters = new NameValueCollection();
        /// parameters["rankAbove"] = "/defect/23456";
		/// OperationResult updateResult = restApi.Update(rallyRef, toUpdate, parameters);
		/// </code>
		/// </example>
		public OperationResult Update(string reference, DynamicJsonObject obj, NameValueCollection parameters)
        {
            return Update(Ref.GetTypeFromRef(reference), Ref.GetOidFromRef(reference), obj, parameters);
        }

        /// <summary>
        /// Update the item described by the specified type and object id with
        /// the fields of the specified object
        /// </summary>
        /// <param name="typePath">the type of the item to be updated</param>
        /// <param name="oid">the object id of the item to be updated</param>
        /// <param name="obj">the object fields to update</param>
        /// <returns>An <see cref="OperationResult"/> describing the status of the request</returns>
        /// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
        /// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
        /// <example>
        /// <code language="C#">
        /// DynamicJsonObject toUpdate = new DynamicJsonObject(); 
        /// toUpdate["Description"] = "This is my defect."; 
        /// OperationResult updateResult = restApi.Update("defect", "12345", toUpdate);
        /// </code>
        /// </example>
        public OperationResult Update(string typePath, string oid, DynamicJsonObject obj)
        {
            return Update(typePath, oid, obj, new NameValueCollection());
        }

        /// <summary>
        /// Update the item described by the specified type and object id with
        /// the fields of the specified object
        /// </summary>
        /// <param name="typePath">the type of the item to be updated</param>
        /// <param name="oid">the object id of the item to be updated</param>
        /// <param name="obj">the object fields to update</param>
        /// <param name="parameters">additional query string parameters to be include on the request</param>
        /// <returns>An <see cref="OperationResult"/> describing the status of the request</returns>
        /// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
        /// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
        /// <example>
        /// <code language="C#">
        /// DynamicJsonObject toUpdate = new DynamicJsonObject(); 
        /// toUpdate["Description"] = "This is my defect."; 
        /// NameValueCollection parameters = new NameValueCollection();
        /// parameters["rankAbove"] = "/defect/23456";
        /// OperationResult updateResult = restApi.Update("defect", "12345", toUpdate, parameters);
        /// </code>
        /// </example>
        public OperationResult Update(string typePath, string oid, DynamicJsonObject obj, NameValueCollection parameters)
        {
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			var result = new OperationResult();
			var data = new DynamicJsonObject();
			data[typePath] = obj;
			dynamic response = DoPost(FormatUpdateUri(typePath, oid, parameters), data);
			if (response.OperationResult["Object"] != null)
				result.Object = response.OperationResult.Object;

			result.Errors.AddRange(DecodeArrayList(response.OperationResult.Errors));
			result.Warnings.AddRange(DecodeArrayList(response.OperationResult.Warnings));
			return result;
		}
        #endregion

        #region Collection Methods

        /// <summary>
        /// Add items to a collection
        /// </summary>
        /// <param name="itemRef">The ref of the object to update e.g. /defect/12345</param>
        /// <param name="collectionName">The name of the collection to be updated e.g. Tasks</param>
        /// <param name="items">The items to add.  These can be references to existing objects or new objects to be created.</param>
        /// <param name="parameters">additional query string parameters to be included on the request</param>
        /// <returns>An <see cref="OperationResult"/> describing the status of the request</returns>
        /// <example>
        /// <code language="C#">
        /// DynamicJsonObject existingTask = new DynamicJsonObject(); 
        /// existingTask["_ref"] = "/task/23456";
        /// DynamicJsonObject newTask = new DynamicJsonObject();
        /// newTask["Name"] = "New Task";
        /// NameValueCollection parameters = new NameValueCollection();
        /// parameters["fetch"] = "FormattedID";
        /// List{DynamicJsonObject} newItems = new List{DynamicJsonObject}() { existingTask, newTask };
        /// OperationResult updateResult = restApi.AddToCollection("/defect/12345", "Tasks", newTasks, parameters);
        /// </code>
        /// </example>
        public OperationResult AddToCollection(string itemRef, string collectionName, List<DynamicJsonObject> items, NameValueCollection parameters)
        {
            return UpdateCollection(itemRef, collectionName, items, parameters, true);
        }

        /// <summary>
        /// Remove items from a collection
        /// </summary>
        /// <param name="itemRef">The ref of the object to update e.g. /defect/12345</param>
        /// <param name="collectionName">The name of the collection to be updated e.g. Tasks</param>
        /// <param name="items">The items to remove.</param>
        /// <param name="parameters">additional query string parameters to be included on the request</param>
        /// <returns>An <see cref="OperationResult"/> describing the status of the request</returns>
        /// <example>
        /// <code language="C#">
        /// DynamicJsonObject existingTask = new DynamicJsonObject(); 
        /// existingTask["_ref"] = "/task/23456";
        /// NameValueCollection parameters = new NameValueCollection();
        /// List{DynamicJsonObject} itemsToRemove = new List{DynamicJsonObject}() { existingTask };
        /// OperationResult updateResult = restApi.RemoveFromCollection("/defect/12345", "Tasks", itemsToRemove, parameters);
        /// </code>
        /// </example>

        public OperationResult RemoveFromCollection(string itemRef, string collectionName, List<DynamicJsonObject> items, NameValueCollection parameters)
        {
            return UpdateCollection(itemRef, collectionName, items, parameters, false);
        }

        private OperationResult UpdateCollection(string itemRef, string collectionName, List<DynamicJsonObject> items, NameValueCollection parameters, bool adding)
        {
            if (ConnectionInfo == null)
            {
                throw new InvalidOperationException(AUTH_ERROR);
            }

            var result = new OperationResult();
            var data = new DynamicJsonObject();
            data["CollectionItems"] = items;
            dynamic response = DoPost(FormatUpdateCollectionUri(adding, itemRef, collectionName, parameters), data);
            if(response.OperationResult.HasMember("Results")) {
                result.Results.AddRange((response.OperationResult.Results as ArrayList).Cast<DynamicJsonObject>());
            }
            result.Errors.AddRange(DecodeArrayList(response.OperationResult.Errors));
            result.Warnings.AddRange(DecodeArrayList(response.OperationResult.Warnings));
            return result;
        }

        #endregion

        #region GetAllowedAttributeValues
        /// <summary>
        /// Get the allowed values for the specified type and attribute
        /// </summary>
        /// <param name="typePath">the type</param>
        /// <param name="attributeName">the attribute to retrieve allowed values for</param>
        /// <returns>Returns a <see cref="DynamicJsonObject"/> containing the allowed values for the specified type and attribute.</returns>
        /// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
        /// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
        /// <example>
        /// <code language="C#">
        /// DynamicJsonObject allowedValues = restApi.GetAllowedAttributeValues("defect", "severity");
        /// </code>
        /// </example>
        public QueryResult GetAllowedAttributeValues(string typePath, string attributeName)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			QueryResult attributes = GetAttributesByType(typePath);
			var attribute = attributes.Results.SingleOrDefault(a => a.ElementName.ToLower() == attributeName.ToLower().Replace(" ", ""));
			if (attribute != null)
			{
				var allowedValues = attribute["AllowedValues"];

				if (IsWsapi2)
				{
					Request allowedValuesRequest = new Request(allowedValues);
					var response = Query(allowedValuesRequest);
					attributes.Results = response.Results;
					attributes.TotalResultCount = allowedValues["Count"];
				}
				else
				{
					attributes.Results = (allowedValues as ArrayList).Cast<object>().ToList<object>();
					attributes.TotalResultCount = allowedValues.Count;
				}
			}

			return attributes;
		}
		#endregion

		#region GetTypes
		/// <summary>
		/// <para><b>Unsupported - DO NOT USE</b></para>
		/// Get the attribute definitions for the specified project or workspace (part of the query string).
		/// <note>This uses an unpublished/unsupported endpoint and should NOT be used by non-Rally applications. 
		/// This endpoint may alter behavior at any point in time.</note>
		/// </summary>
		/// <param name="queryString">The query string to get types for</param>
		/// <returns>The type definitions for the specified query</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <para><b>Unsupported - DO NOT USE</b></para>
		/// <note>This uses an unpublished/unsupported endpoint and should NOT be used by non-Rally applications. 
		/// This endpoint may alter behavior at any point in time.</note>
		/// </example>
		public CacheableQueryResult GetTypes(string queryString)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			if (!IsWsapi2)
				throw new InvalidOperationException("This method requires WSAPI 2.0");

			Uri uri = GetFullyQualifiedV2xSchemaUri(queryString);
			bool isCachedResult;
			var response = DoGetCacheable(uri, out isCachedResult);

			return new CacheableQueryResult(response["QueryResult"], isCachedResult);
		}
		#endregion

		#region GetAttributesByType
		/// <summary>
		/// Get the attribute definitions for the specified type
		/// </summary>
		/// <param name="type">The type to get attributes for</param>
		/// <returns>Returns a <see cref="QueryResult"/> object containing the attribute definitions for the specified type.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		/// <example>
		/// <code language="C#">
		/// DynamicJsonObject allowedValues = restApi.GetAllowedAttributeValues("defect", "severity");
		/// </code>
		/// </example>
		public QueryResult GetAttributesByType(string type)
		{
			if (ConnectionInfo == null)
				throw new InvalidOperationException(AUTH_ERROR);

			var typeDefRequest = new Request("TypeDefinition");
			typeDefRequest.Fetch = new List<string>() { "Attributes" };
			typeDefRequest.Query = new Query("TypePath", RestApi.Query.Operator.Equals, type.Replace(" ", ""));
			var result = Query(typeDefRequest);
			var typeDefResult = result.Results.FirstOrDefault();
			if (typeDefResult != null)
			{
				var attributes = typeDefResult["Attributes"];
				if (IsWsapi2)
				{
					Request attributeRequest = new Request(attributes);
					var response = Query(attributeRequest);
					result.Results = response.Results;
					result.TotalResultCount = attributes["Count"];
				}
				else
				{
					result.Results = (attributes as ArrayList).Cast<object>().ToList<object>();
					result.TotalResultCount = attributes.Count;
				}
			}

			return result;
		}
		#endregion

		#region DownloadAttachment
		/// <summary>
		/// Downloads an attachment from Rally.
		/// </summary>
		/// <param name="relativeUrl">The relative URL to the attachment.</param>
		/// <returns>The result of the request.</returns>
		/// <example>
		/// <code language="C#">
		/// string relativeUrl = "/slm/attachment/12345678900/image_file_name.jpg"
		/// AttachmentResult attachmentResult = DownloadAttachment(relativeUrl);
		/// </code>
		/// </example>
		public AttachmentResult DownloadAttachment(string relativeUrl)
		{
			if (relativeUrl.Length < 5)
				throw new ArgumentOutOfRangeException("A valid relative URL must be provided.");

			AttachmentResult result = new AttachmentResult();
			string expectedUrl = String.Format("{0}{1}", httpService.Server.AbsoluteUri, relativeUrl.Substring(1));
			Uri uri = new Uri(expectedUrl);
			result.FileContents = httpService.Download(uri, GetProcessedHeaders());
			return result;
		}
        #endregion

        #region Helper Methods

        #region Format Uris

        private string ToQueryString(NameValueCollection parameters)
        {
            if(parameters == null || parameters.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder("?");
            foreach (string key in parameters.AllKeys)
            {
                sb.AppendFormat("{0}={1}&", key, HttpUtility.UrlEncode(parameters[key]));
            }
            return sb.ToString().TrimEnd('&');
        }

        internal Uri FormatCreateUri(string typePath, NameValueCollection parameters)
		{
            return new Uri(httpService.Server.AbsoluteUri + "slm/webservice/" + WsapiVersion + "/" + typePath + "/create.js" + ToQueryString(parameters));
		}

		internal Uri FormatUpdateUri(string typePath, string objectId, NameValueCollection parameters)
		{
			return new Uri(httpService.Server.AbsoluteUri + "slm/webservice/" + WsapiVersion + "/" + typePath + "/" + objectId + ".js" + ToQueryString(parameters));
		}

        internal Uri FormatUpdateCollectionUri(bool isAdding, string itemRef, string collectionName, NameValueCollection parameters)
        {
            return new Uri(httpService.Server.AbsoluteUri + "slm/webservice/" + WsapiVersion + "/" + Ref.GetTypeFromRef(itemRef) + "/" + Ref.GetOidFromRef(itemRef) + "/" + collectionName + (isAdding ? "/add" : "/remove") + ToQueryString(parameters));
        }
        #endregion

        #region DoGetCacheable
        /// <summary>
        /// Gets a cacheable response.
        /// </summary>
        /// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
        /// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
        private DynamicJsonObject DoGetCacheable(Uri uri, out bool isCachedResult)
		{
			return httpService.GetCacheable(uri, out isCachedResult, GetProcessedHeaders());
		}
		#endregion

		#region DoGetAsPost
		/// <summary>
		/// Performs get as a post action.
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		private DynamicJsonObject DoGetAsPost(Request request, bool retry = true, int retryCounter = 1)
		{
			int retrySleepTime = 1000;
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11
																							 | SecurityProtocolType.Tls12;
				ServicePointManager.Expect100Continue = true;
				Dictionary<string, string> data = request.GetDataToSend();
				Uri uri = GetFullyQualifiedUri(request.ShortRequestUrl);
				Dictionary<string, string> processedHeaders = GetProcessedHeaders();
				DynamicJsonObject response = serializer.Deserialize(httpService.GetAsPost(GetSecuredUri(uri), data, processedHeaders));

				if (retry && response[response.Fields.First()].Errors.Count > 0 && retryCounter < this.maxRetries)
				{
					ConnectionInfo.SecurityToken = GetSecurityToken();
					httpService = new HttpService(authManger, ConnectionInfo);
					Thread.Sleep(retrySleepTime * retryCounter);
					return DoGetAsPost(request, true, ++retryCounter);
				}

				return response;
			}
			catch (Exception)
			{
				if (retryCounter < this.maxRetries)
				{
					Thread.Sleep(retrySleepTime * retryCounter);
					return DoGetAsPost(request, true, ++retryCounter);
				}
				throw;
			}
		}
		#endregion

		#region DoGet
		/// <summary>
		/// Performs a get action.
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		private DynamicJsonObject DoGet(Uri uri, bool retry = true, int retryCounter = 1)
		{
			int retrySleepTime = 1000;
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11
																							 | SecurityProtocolType.Tls12;
				ServicePointManager.Expect100Continue = true;
				Dictionary<string, string> processedHeaders = GetProcessedHeaders();
				DynamicJsonObject response = serializer.Deserialize(httpService.Get(uri, processedHeaders));

				if (retry && response[response.Fields.First()].Errors.Count > 0 && retryCounter < this.maxRetries)
				{
					ConnectionInfo.SecurityToken = GetSecurityToken();
					httpService = new HttpService(authManger, ConnectionInfo);
					Thread.Sleep(retrySleepTime * retryCounter);
					return DoGet(uri, true, ++retryCounter);
				}

				return response;
			}
			catch (Exception)
			{
				if (retryCounter < this.maxRetries)
				{
					Thread.Sleep(retrySleepTime * retryCounter);
					return DoGet(uri, true, ++retryCounter);
				}
				throw;
			}
		}
		#endregion

		#region DoPost
		/// <summary>
		/// Performs a post action.  If first action fails there will occur up to 10 retries each backing off an incrementing number of seconds (wait 1 second, retry, wait 2 seconds, retry, etc).
		/// 
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		private DynamicJsonObject DoPost(Uri uri, DynamicJsonObject data, bool retry = true, int retryCounter = 1)
		{
			int retrySleepTime = 1000;
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
				ServicePointManager.Expect100Continue = true;
				Dictionary<string, string> processedHeaders = GetProcessedHeaders();
				var response = serializer.Deserialize(httpService.Post(GetSecuredUri(uri), serializer.Serialize(data), processedHeaders));

				if (retry && response[response.Fields.First()].Errors.Count > 0 && retryCounter < this.maxRetries)
				{
					ConnectionInfo.SecurityToken = GetSecurityToken();
					httpService = new HttpService(authManger, ConnectionInfo);
					Thread.Sleep(retrySleepTime * retryCounter);
					return DoPost(uri, data, true, ++retryCounter);
				}

				return response;
			}
			catch (Exception)
			{
				if (retryCounter < this.maxRetries)
				{
					Thread.Sleep(retrySleepTime * retryCounter);
					return DoPost(uri, data, true, ++retryCounter);
				}
				throw;
			}
		}
		#endregion

		#region GetSecuredUri
		private Uri GetSecuredUri(Uri uri)
		{
			if (ConnectionInfo.AuthType == AuthorizationType.ApiKey)
				return uri;

			if (IsWsapi2)
			{
				if (String.IsNullOrEmpty(ConnectionInfo.SecurityToken))
				{
					ConnectionInfo.SecurityToken = GetSecurityToken();
				}

				UriBuilder builder = new UriBuilder(uri);
				string csrfToken = String.Format("key={0}", ConnectionInfo.SecurityToken);
				if (String.IsNullOrEmpty(builder.Query))
				{
					builder.Query = csrfToken;
				}
				else
				{
					builder.Query = builder.Query.Substring(1) + "&" + csrfToken;
				}

				return builder.Uri;
			}
			return uri;
		}
		#endregion

		#region GetSecurityToken
		/// <summary>
		/// Gets a security token from Rally.
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		private string GetSecurityToken()
		{
			try
			{
				DynamicJsonObject securityTokenResponse = DoGet(new Uri(GetFullyQualifiedRef(SECURITY_ENDPOINT)), this.maxRetries > 1);
				return securityTokenResponse["OperationResult"]["SecurityToken"];
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region GetProcessedHeaders
		internal Dictionary<string, string> GetProcessedHeaders()
		{
			var result = new Dictionary<string, string>();
			try
			{
				foreach (HeaderType headerType in Headers.Keys)
				{
					string output = null;
					string value = Headers[headerType];
					FieldInfo fieldInfo = headerType.GetType().GetField(headerType.ToString());
					StringValue[] attrs = fieldInfo.GetCustomAttributes(typeof(StringValue), false) as StringValue[];
					if (attrs.Length > 0)
						output = attrs[0].Value;

					result.Add(output, value);
				}
			}
			catch
			{
				// Swallow exception for headers.
			}

			return result;
		}
		#endregion

		#region GetFullyQualifiedUri
		/// <summary>
		/// Ensure the specified ref is fully qualified with the full WSAPI URL
		/// </summary>
		/// <param name="aRef">A Rally object ref</param>
		/// <returns>The fully qualified ref</returns>
		private Uri GetFullyQualifiedUri(string aRef)
		{
			return new Uri(GetFullyQualifiedRef(aRef));
		}
		#endregion

		#region GetFullyQualifiedRef
		/// <summary>
		/// Ensure the specified ref is fully qualified with the full WSAPI URL
		/// </summary>
		/// <param name="aRef">A Rally object ref</param>
		/// <returns>The fully qualified ref</returns>
		private string GetFullyQualifiedRef(string aRef)
		{
			if (!aRef.StartsWith(WebServiceUrl))
				return String.Format("{0}{1}", WebServiceUrl, aRef);
			else
				return aRef;
		}
		#endregion

		#region GetFullyQualifiedV2xSchemaUri
		/// <summary>
		/// Ensure the specified ref is fully qualified with the full WSAPI URL
		/// </summary>
		/// <param name="aRef">A Rally object ref</param>
		/// <returns>The fully qualified ref</returns>
		private Uri GetFullyQualifiedV2xSchemaUri(string aRef)
		{
			// HACK: This is a total hack to access a custom API that does not follow standard endpoint behavior.
			// The schema endpoint has a different base and version, and therefore requires this workaround.
			return new Uri(GetFullyQualifiedRef(aRef).Replace("webservice/v2.0", "schema/v2.x"));
		}
		#endregion

		#region DecodeArrayList
		private static IEnumerable<string> DecodeArrayList(IEnumerable list)
		{
			return list.Cast<string>();
		}
		#endregion

		#endregion
	}
}