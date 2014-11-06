using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Rally.RestApi.Response;
using System.Text.RegularExpressions;
using Rally.RestApi.Web;
using Rally.RestApi.Connection;
using Rally.RestApi.Json;
using Rally.RestApi.Sso;
using System.Threading;

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
		public const string SECURITY_ENDPOINT = "/security/authorize";
		/// <summary>
		/// Is SSO in progress?
		/// </summary>
		public bool IsSsoInProgress { get; private set; }

		#region Enumeration: AuthenticationResult
		/// <summary>
		/// Enumeration of the different authentication results that may occur.
		/// </summary>
		public enum AuthenticationResult
		{
			/// <summary>
			/// The user is authenticated.
			/// </summary>
			Authenticated,
			/// <summary>
			/// The user needs to perform SSO authentication.
			/// </summary>
			PendingSSO,
			/// <summary>
			/// The user is not authorized.
			/// </summary>
			NotAuthorized,
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
		/// The default server to use: (https://rally1.rallydev.com)
		/// </summary>
		public const string DEFAULT_SERVER = "https://rally1.rallydev.com";
		#endregion

		#region Properties and Fields
		private ISsoDriver ssoDriver;
		private HttpService httpService;
		private readonly DynamicJsonSerializer serializer = new DynamicJsonSerializer();
		/// <summary>
		/// The HTTP headers to be included on all REST requests
		/// </summary>
		public Dictionary<HeaderType, string> Headers { get; private set; }
		/// <summary>
		/// The connection info thsi API is using.
		/// </summary>
		internal ConnectionInfo ConnectionInfo { get; private set; }
		/// <summary>
		/// The WSAPI version we are talking to.
		/// </summary>
		public virtual String WsapiVersion { get; set; }
		/// <summary>
		/// Is this connection using WSAPI 2?
		/// </summary>
		internal bool IsWsapi2 { get { return !new Regex("^1[.]\\d+").IsMatch(WsapiVersion); } }
		#endregion

		/// <summary>
		/// An event that indicates changes to SSO authentication.
		/// </summary>
		public event SsoResults SsoResults;

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
		/// <param name="ssoDriver">The SSO Driver to use when authentication requires it. If no driver is provided, SSO will not be enabled.</param>
		/// <param name="webServiceVersion">The WSAPI version to use (defaults to DEFAULT_WSAPI_VERSION)</param>
		public RallyRestApi(ISsoDriver ssoDriver = null, string webServiceVersion = DEFAULT_WSAPI_VERSION)
		{
			if (ssoDriver == null)
				ssoDriver = new SsoNotAllowedDriver();

			this.ssoDriver = ssoDriver;

			WsapiVersion = webServiceVersion;
			if (String.IsNullOrWhiteSpace(WsapiVersion))
				WsapiVersion = DEFAULT_WSAPI_VERSION;
		}
		#endregion

		#region Authenticate
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="userName">The username to be used for access</param>
		/// <param name="zSessionID">The ZSessionID to be used for access. This would have been provided by Rally on a previous call.</param>
		/// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
		public AuthenticationResult AuthenticateWithZSessionID(string userName, string zSessionID,
			string rallyServer = DEFAULT_SERVER, WebProxy proxy = null, bool allowSSO = true)
		{
			if (String.IsNullOrWhiteSpace(rallyServer))
				rallyServer = DEFAULT_SERVER;

			if (!ssoDriver.IsSsoAuthorized)
				throw new InvalidOperationException("ZSessionID authentication is only supported with a valid SSO provider.");

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.ZSessionID;
			connectionInfo.UserName = userName;
			connectionInfo.ZSessionID = zSessionID;
			connectionInfo.Server = new Uri(rallyServer);
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, allowSSO);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="apiKey">The API key to be used for access</param>
		/// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
		public AuthenticationResult AuthenticateWithApiKey(string apiKey, string rallyServer = DEFAULT_SERVER, WebProxy proxy = null, bool allowSSO = true)
		{
			if (String.IsNullOrWhiteSpace(rallyServer))
				rallyServer = DEFAULT_SERVER;

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.ApiKey;
			connectionInfo.ApiKey = apiKey;
			connectionInfo.Server = new Uri(rallyServer);
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, allowSSO);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="apiKey">The API key to be used for access</param>
		/// <param name="serverUrl">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
		public AuthenticationResult AuthenticateWithApiKey(string apiKey, Uri serverUrl, WebProxy proxy = null, bool allowSSO = true)
		{
			if (serverUrl == null)
				serverUrl = new Uri(DEFAULT_SERVER);

			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.AuthType = AuthorizationType.ApiKey;
			connectionInfo.ApiKey = apiKey;
			connectionInfo.Server = serverUrl;
			connectionInfo.Proxy = proxy;
			return AuthenticateWithConnectionInfo(connectionInfo, allowSSO);
		}
		/// <summary>
		/// Authenticates against Rally with the specified credentials
		/// </summary>
		/// <param name="username">The username to be used for access</param>
		/// <param name="password">The password to be used for access</param>
		/// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
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
		/// <param name="username">The username to be used for access</param>
		/// <param name="password">The password to be used for access</param>
		/// <param name="serverUrl">The Rally server to use (defaults to DEFAULT_SERVER)</param>
		/// <param name="proxy">Optional proxy configuration</param>
		/// <param name="allowSSO">Is SSO authentication allowed for this call? It can be useful to disable this during startup processes.</param>
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
		private AuthenticationResult AuthenticateWithConnectionInfo(ConnectionInfo connectionInfo, bool allowSSO)
		{
			this.ConnectionInfo = connectionInfo;
			httpService = new HttpService(ssoDriver, connectionInfo);
			httpService.SsoResults += SsoCompleted;

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

			try
			{
				GetCurrentUser("Name");
				return AuthenticationResult.Authenticated;
			}
			catch
			{
				try
				{
					if ((allowSSO) && (!httpService.PerformSsoAuthentication()))
					{
						connectionInfo = null;
						httpService = null;
						throw;
					}
				}
				finally
				{
					IsSsoInProgress = true;
				}

				return AuthenticationResult.PendingSSO;
			}
		}
		#endregion

		#region BlockIfSsoInProgress
		private void BlockIfSsoInProgress()
		{
			while (IsSsoInProgress)
			{
				Thread.CurrentThread.Join(100);
			}
		}
		#endregion

		#region SsoCompleted
		private void SsoCompleted(bool success, string zSessionID)
		{
			if (success)
			{
				ConnectionInfo.AuthType = AuthorizationType.ZSessionID;
				ConnectionInfo.ZSessionID = zSessionID;
			}

			IsSsoInProgress = false;

			if (SsoResults != null)
				SsoResults.Invoke(success, zSessionID);
		}
		#endregion

		#region SetDefaultConnectionLimit
		/// <summary>
		/// Sets the default maximum concurrent connection limit for this application.
		/// <para>Note: This will affect all connections that use Service Point.</para>
		/// </summary>
		/// <param name="maxConnections">The maximum number of concurrent connections. Allowed values are between 1 and 25.</param>
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
		public DynamicJsonObject Post(String relativeUri, DynamicJsonObject data)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

			Uri uri = new Uri(String.Format("{0}slm/webservice/{1}/{2}", httpService.Server.AbsoluteUri, WsapiVersion, relativeUri));
			string postData = serializer.Serialize(data);
			return serializer.Deserialize(httpService.Post(uri, postData, GetProcessedHeaders()));
		}
		#endregion

		#region DoDelete
		private DynamicJsonObject DoDelete(Uri uri, bool retry = true)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		/// <returns>The results of the read operation</returns>
		public QueryResult Query(Request request)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

			var response = DoGet(GetFullyQualifiedUri(request.RequestUrl));
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

			Trace.TraceInformation("The number of threaded requests is : {0}", subsequentQueries.Count);

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
		/// <returns></returns>
		public dynamic GetSubscription(params string[] fetchedFields)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		/// <returns>The requested object</returns>
		public dynamic GetByReference(string typePath, long oid, params string[] fetchedFields)
		{
			return GetByReference(string.Format("/{0}/{1}", typePath, oid), fetchedFields);
		}

		/// <summary>
		/// Get the object described by the specified reference.
		/// </summary>
		/// <param name="aRef">the reference</param>
		/// <param name="fetchedFields">the list of object fields to be fetched</param>
		/// <returns>The requested object</returns>
		public dynamic GetByReference(string aRef, params string[] fetchedFields)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		/// <returns>The requested object</returns>
		public dynamic GetByReferenceAndWorkspace(string aRef, string workspaceRef, params string[] fetchedFields)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		public OperationResult Delete(string typePath, long oid)
		{
			return Delete(null, string.Format("/{0}/{1}", typePath, oid));
		}

		/// <summary>
		/// Delete the object described by the specified reference.
		/// </summary>
		/// <param name="aRef">the reference</param>
		/// <returns>An OperationResult with information on the status of the request</returns>
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
		public OperationResult Delete(string workspaceRef, string aRef)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		/// <returns></returns>
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
		/// <returns></returns>
		public CreateResult Create(string workspaceRef, string typePath, DynamicJsonObject obj)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

			var data = new DynamicJsonObject();
			data[typePath] = obj;
			DynamicJsonObject response = DoPost(FormatCreateUri(workspaceRef, typePath), data);
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
		/// <returns>An OperationResult describing the status of the request</returns>
		public OperationResult Update(string reference, DynamicJsonObject obj)
		{
			return Update(Ref.GetTypeFromRef(reference), Ref.GetOidFromRef(reference), obj);
		}

		/// <summary>
		/// Update the item described by the specified type and object id with
		/// the fields of the specified object
		/// </summary>
		/// <param name="typePath">the type of the item to be updated</param>
		/// <param name="oid">the object id of the item to be updated</param>
		/// <param name="obj">the object fields to update</param>
		/// <returns>An OperationResult describing the status of the request</returns>
		public OperationResult Update(string typePath, string oid, DynamicJsonObject obj)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

			var result = new OperationResult();
			var data = new DynamicJsonObject();
			data[typePath] = obj;
			dynamic response = DoPost(FormatUpdateUri(typePath, oid), data);
			result.Object = response.OperationResult.Object;
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
		/// <param name="attributeName">the attribute to retireve allowed values for</param>
		/// <returns>The allowed values for the specified attribute</returns>
		public QueryResult GetAllowedAttributeValues(string typePath, string attributeName)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		/// Get the attribute definitions for the specified type
		/// </summary>
		/// <param name="queryString">The query string to get types for</param>
		/// <returns>The type definitions for the specified query</returns>
		public CacheableQueryResult GetTypes(string queryString)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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
		/// <returns>The attribute definitions for the specified type</returns>
		public QueryResult GetAttributesByType(string type)
		{
			BlockIfSsoInProgress();
			if (ConnectionInfo == null)
				throw new InvalidOperationException("You must authenticate against Rally prior to performing any data operations.");

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

		/// <summary>
		/// Downloads an attachment from Rally.
		/// </summary>
		/// <param name="relativeUrl">The relative URL to the attachment.</param>
		/// <returns>The result of the request.</returns>
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

		#region Helper Methods

		#region FormatCreateUri
		internal Uri FormatCreateUri(string workspaceRef, string typePath)
		{
			String workspaceClause = workspaceRef == null ? "" : "?workspace=" + workspaceRef;
			return new Uri(httpService.Server.AbsoluteUri + "slm/webservice/" + WsapiVersion + "/" + typePath + "/create.js" + workspaceClause);
		}
		#endregion

		#region FormatUpdateUri
		internal Uri FormatUpdateUri(string typePath, string objectId)
		{
			return
					new Uri(httpService.Server.AbsoluteUri + "slm/webservice/" + WsapiVersion + "/" + typePath + "/" + objectId +
									".js");
		}
		#endregion

		#region DoGetCacheable
		private DynamicJsonObject DoGetCacheable(Uri uri, out bool isCachedResult)
		{
			return httpService.GetCacheable(uri, out isCachedResult, GetProcessedHeaders());
		}
		#endregion

		#region DoGet
		private DynamicJsonObject DoGet(Uri uri)
		{
			return serializer.Deserialize(httpService.Get(uri, GetProcessedHeaders()));
		}
		#endregion

		#region DoPost
		private DynamicJsonObject DoPost(Uri uri, DynamicJsonObject data, bool retry = true)
		{
			var response = serializer.Deserialize(httpService.Post(GetSecuredUri(uri), serializer.Serialize(data), GetProcessedHeaders()));
			if (retry && ConnectionInfo.SecurityToken != null && response[response.Fields.First()].Errors.Count > 0)
			{
				ConnectionInfo.SecurityToken = null;
				return DoPost(uri, data, false);
			}
			return response;
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
					builder.Query += "&" + csrfToken;
				}

				return builder.Uri;
			}
			return uri;
		}
		#endregion

		#region GetSecurityToken
		private string GetSecurityToken()
		{
			try
			{
				DynamicJsonObject securityTokenResponse = DoGet(new Uri(GetFullyQualifiedRef(SECURITY_ENDPOINT)));
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
			return result;
		}
		#endregion

		#region GetFullyQualifiedUri
		/// <summary>
		/// Ensure the specified ref is fully qualified
		/// with the full WSAPI url
		/// </summary>
		/// <param name="aRef">A Rally object ref</param>
		/// <returns>The fully qualified ref</returns>
		protected Uri GetFullyQualifiedUri(string aRef)
		{
			return new Uri(GetFullyQualifiedRef(aRef));
		}
		#endregion

		#region GetFullyQualifiedRef
		/// <summary>
		/// Ensure the specified ref is fully qualified
		/// with the full WSAPI url
		/// </summary>
		/// <param name="aRef">A Rally object ref</param>
		/// <returns>The fully qualified ref</returns>
		protected string GetFullyQualifiedRef(string aRef)
		{
			if (!aRef.StartsWith(WebServiceUrl))
				return String.Format("{0}{1}", WebServiceUrl, aRef);
			else
				return aRef;
		}
		#endregion

		#region GetFullyQualifiedV2xSchemaUri
		/// <summary>
		/// Ensure the specified ref is fully qualified with the full WSAPI url
		/// </summary>
		/// <param name="aRef">A Rally object ref</param>
		/// <returns>The fully qualified ref</returns>
		protected Uri GetFullyQualifiedV2xSchemaUri(string aRef)
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