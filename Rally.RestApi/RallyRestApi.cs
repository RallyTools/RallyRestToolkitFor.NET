using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Rally.RestApi.Response;
using System.Text.RegularExpressions;

namespace Rally.RestApi
{
    /// <summary>
    /// The main interface to the Rally REST API
    /// </summary>
    public class RallyRestApi
    {
        #region Properties

        private const int MAX_THREADS_ALLOWED = 6;

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

        /// <summary>
        /// The default WSAPI version to use
        /// </summary>
        public const string DEFAULT_WSAPI_VERSION = "v2.0";

        /// <summary>
        /// The default server to use: (https://rally1.rallydev.com)
        /// </summary>
        public const string DEFAULT_SERVER = "https://rally1.rallydev.com";

        /// <summary>
        /// The HTTP headers to be included on all REST requests
        /// </summary>
        public readonly Dictionary<HeaderType, string> Headers = new Dictionary<HeaderType, string>
                                                                     {
                                                                         {HeaderType.Library, ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(
                                                                                typeof(RallyRestApi).Assembly, typeof(AssemblyTitleAttribute), false)).Title + " v" + typeof(RallyRestApi).Assembly.GetName().Version.ToString()},
                                                                         {HeaderType.Vendor, ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(
                                                                                typeof(RallyRestApi).Assembly, typeof(AssemblyCompanyAttribute), false)).Company},
                                                                         {HeaderType.Name, ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(
                                                                                typeof(RallyRestApi).Assembly, typeof(AssemblyTitleAttribute), false)).Title},
                                                                         {HeaderType.Version, typeof(RallyRestApi).Assembly.GetName().Version.ToString()}
                                                                     };

        private readonly DynamicJsonSerializer serializer = new DynamicJsonSerializer();
        private readonly string wsapiVersion;
        private string securityToken;
        internal HttpService Service { get; set; }

        internal Dictionary<string, string> GetProcessedHeaders()
        {
            var result = new Dictionary<string, string>();
            foreach (var pair in Headers)
            {
                result.Add(getStringValue(pair.Key), pair.Value);
            }
            return result;
        }

        internal class StringValue : System.Attribute
        {
            private string _value;

            public StringValue(string value)
            {
                _value = value;
            }

            public string Value
            {
                get { return _value; }
            }
        }

        internal static string getStringValue(Enum value)
        {
            string output = null;
            FieldInfo fi = value.GetType().GetField(value.ToString());
            StringValue[] attrs = fi.GetCustomAttributes(typeof(StringValue), false) as StringValue[];
            if (attrs.Length > 0)
                output = attrs[0].Value;
            return output;
        }

        #endregion

        public RallyRestApi(
            string username, 
            string password, 
            string rallyServer = DEFAULT_SERVER,
            string webServiceVersion = DEFAULT_WSAPI_VERSION,
            WebProxy proxy = null,
            CancellationToken? cancellationToken = null
        )
            : this(username, password, new Uri(rallyServer), webServiceVersion, proxy, cancellationToken)
        {        
        }

        /// <summary>
        /// Construct a new RallyRestApi with the specified
        /// username, password, server and WSAPI version
        /// </summary>
        /// <param name="username">The username to be used for access</param>
        /// <param name="password">The password to be used for access</param>
        /// <param name="serverUrl">The Rally server to use (defaults to DEFAULT_SERVER)</param>
        /// <param name="webServiceVersion">The WSAPI version to use (defaults to DEFAULT_WSAPI_VERSION)</param>
        /// <param name="proxy">Optional proxy configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public RallyRestApi(
            string username,
            string password,
            Uri serverUrl,
            string webServiceVersion = DEFAULT_WSAPI_VERSION,
            WebProxy proxy = null,
            CancellationToken? cancellationToken = null
        )
            : this(new ConnectionInfo() {authType = AuthorizationType.Basic,
                                         username = username,
                                         password = password,
                                         server = serverUrl,
                                         wsapiVersion = webServiceVersion,
                                         proxy = proxy}, cancellationToken)
        {
        }

        /// <summary>
        /// Construct a new RallyRestApi from the specified ConnectionInfo
        /// </summary>
        /// <param name="connectionInfo">ConnectionInfo</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        public RallyRestApi(IConnectionInfo connectionInfo, CancellationToken? cancellationToken = null)
        {
            Service = new HttpService(connectionInfo, cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None);
            wsapiVersion = connectionInfo.wsapiVersion ?? DEFAULT_WSAPI_VERSION;
        }

        /// <summary>
        /// The full WSAPI url
        /// </summary>
        public string WebServiceUrl
        {
            get { return Service.Server.AbsoluteUri + "slm/webservice/" + wsapiVersion; }
        }

        #region Non Public

        private static IEnumerable<string> DecodeArrayList(IEnumerable list)
        {
            return list.Cast<string>();
        }

        /// <summary>
        /// Ensure the specified ref is fully qualified
        /// with the full WSAPI url
        /// </summary>
        /// <param name="aRef">A Rally object ref</param>
        /// <returns>The fully qualified ref</returns>
        protected string GetFullyQualifiedRef(string aRef)
        {
            if (!aRef.StartsWith(WebServiceUrl))
            {
                aRef = WebServiceUrl + aRef;
            }

            return aRef;
        }

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

        internal Uri FormatCreateUri(string workspaceRef, string typePath)
        {
            String workspaceClause = workspaceRef == null ? "" : "?workspace=" + workspaceRef;
            return new Uri(Service.Server.AbsoluteUri + "slm/webservice/" + wsapiVersion + "/" + typePath + "/create.js" + workspaceClause);
        }

        internal Uri FormatUpdateUri(string typePath, string objectId)
        {
            return
                new Uri(Service.Server.AbsoluteUri + "slm/webservice/" + wsapiVersion + "/" + typePath + "/" + objectId +
                        ".js");
        }

        DynamicJsonObject DoGet(Uri uri)
        {
            return serializer.Deserialize(Service.Get(uri, GetProcessedHeaders()));
        }

        public DynamicJsonObject post(String relativeUri, DynamicJsonObject data)
        {
            Uri uri = new Uri(String.Format("{0}slm/webservice/{1}/{2}", Service.Server.AbsoluteUri, wsapiVersion, relativeUri));
            string postData = serializer.Serialize(data);
            return serializer.Deserialize(Service.Post(uri, postData, GetProcessedHeaders()));
        }

        DynamicJsonObject DoPost(Uri uri, DynamicJsonObject data)
        {
            return DoPost(uri, data, true);
        }

        DynamicJsonObject DoPost(Uri uri, DynamicJsonObject data, bool retry)
        {
            var response = serializer.Deserialize(Service.Post(GetSecuredUri(uri), serializer.Serialize(data), GetProcessedHeaders()));
            if (retry && securityToken != null && response[response.Fields.First()].Errors.Count > 0)
            {
                securityToken = null;
                return DoPost(uri, data, false);
            }
            return response;
        }

        DynamicJsonObject DoDelete(Uri uri)
        {
            return DoDelete(uri, true);
        }

        DynamicJsonObject DoDelete(Uri uri, bool retry)
        {
            var response = serializer.Deserialize(Service.Delete(GetSecuredUri(uri), GetProcessedHeaders()));
            if (retry && securityToken != null && response[response.Fields.First()].Errors.Count > 0)
            {
                securityToken = null;
                return DoDelete(uri, false);
            }
            return response;
        }

        internal bool IsWsapi2
        {
            get
            {
                return !new Regex("^1[.]\\d+").IsMatch(wsapiVersion);
            }
        }

        Uri GetSecuredUri(Uri uri)
        {
            if (IsWsapi2)
            {
                if (string.IsNullOrEmpty(securityToken))
                {
                    securityToken = GetSecurityToken();
                }

                UriBuilder builder = new UriBuilder(uri);
                string csrfToken = string.Format("key={0}", securityToken);
                if (string.IsNullOrEmpty(builder.Query))
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

        string GetSecurityToken()
        {
            try
            {
                DynamicJsonObject securityTokenResponse = DoGet(new Uri(GetFullyQualifiedRef("/security/authorize")));
                return securityTokenResponse["OperationResult"]["SecurityToken"];
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<object> GetCollection(object arr)
        {
            var list = arr as ArrayList;
            return list.Cast<object>();
        }

        #endregion

        /// <summary>
        /// Perform a read against the WSAPI operation based
        /// on the data in the specified request
        /// </summary>
        /// <param name="request">The request configuration</param>
        /// <returns>The results of the read operation</returns>
        public QueryResult Query(Request request)
        {
            var response = DoGet(GetFullyQualifiedUri(request.RequestUrl));
            var result = new QueryResult(response["QueryResult"]);
            int maxResultsAllowed = Math.Min(request.Limit, result.TotalResultCount);
            int alreadyDownloadedItems = request.Start - 1 + request.PageSize;
            var subsequentQueries = new List<Request>();

            while ((maxResultsAllowed - alreadyDownloadedItems) > 0)
            {
                var newRequest = request.Clone(request.Start + request.PageSize);
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

        /// <summary>
        /// Get the current user
        /// </summary>
        /// <param name="fetchedFields">p</param>
        /// <returns></returns>
        public dynamic GetCurrentUser(params string[] fetchedFields)
        {
            return GetByReference("/user.js", fetchedFields);
        }

        /// <summary>
        /// Get the current subscription
        /// </summary>
        /// <param name="fetchedFields">An optional list of fields to be fetched</param>
        /// <returns></returns>
        public dynamic GetSubscription(params string[] fetchedFields)
        {
            return GetByReference("/subscription.js", fetchedFields);
        }

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

        /// <summary>
        /// Create an object of the specified type from the specified object
        /// </summary>
        /// <param name="workspaceRef">the workspace into which the object should be created.  Null means that the server will pick a workspace.</param>
        /// <param name="typePath">the type to be created</param>
        /// <param name="obj">the object to be created</param>
        /// <returns></returns>
        public CreateResult Create(string workspaceRef, string typePath, DynamicJsonObject obj)
        {
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
            var result = new OperationResult();
            var data = new DynamicJsonObject();
            data[typePath] = obj;
            dynamic response = DoPost(FormatUpdateUri(typePath, oid), data);
            result.Errors.AddRange(DecodeArrayList(response.OperationResult.Errors));
            result.Warnings.AddRange(DecodeArrayList(response.OperationResult.Warnings));
            return result;
        }

        /// <summary>
        /// Get the allowed values for the specified type and attribute
        /// </summary>
        /// <param name="typePath">the type</param>
        /// <param name="attributeName">the attribute to retireve allowed values for</param>
        /// <returns>The allowed values for the specified attribute</returns>
        public QueryResult GetAllowedAttributeValues(string typePath, string attributeName)
        {
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

        /// <summary>
        /// Get the attribute definitions for the specified type
        /// </summary>
        /// <param name="type">The type to get attributes for</param>
        /// <returns>The attribute definitions for the specified type</returns>
        public QueryResult GetAttributesByType(string type)
        {
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
    }
}