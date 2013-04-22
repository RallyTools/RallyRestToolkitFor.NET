using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Rally.RestApi.Response;

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
        /// The default WSAPI version to use: 'x' means 'latest'
        /// </summary>
        public const string DEFAULT_WSAPI_VERSION = "x";

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
            StringValue[] attrs = fi.GetCustomAttributes(typeof(StringValue),false) as StringValue[];
            if (attrs.Length > 0)
                output = attrs[0].Value;
            return output;
        }

        #endregion

        /// <summary>
        /// Construct a new RallyRestApi with the specified
        /// username, password, server and WSAPI version
        /// </summary>
        /// <param name="username">The username to be used for access</param>
        /// <param name="password">The password to be used for access</param>
        /// <param name="rallyServer">The Rally server to use (defaults to DEFAULT_SERVER)</param>
        /// <param name="webServiceVersion">The WSAPI version to use (defaults to DEFAULT_WSAPI_VERSION)</param>
        /// <param name="proxy">Optional proxy configuration</param>
        public RallyRestApi(string username, string password, string rallyServer = DEFAULT_SERVER,
                            string webServiceVersion = DEFAULT_WSAPI_VERSION, WebProxy proxy=null)
            : this(username, password, new Uri(rallyServer), webServiceVersion, proxy)
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
        public RallyRestApi(string username, string password, Uri serverUrl,
                            string webServiceVersion = DEFAULT_WSAPI_VERSION, WebProxy proxy=null)
        {
            Service = new HttpService(username, password, serverUrl, proxy);
            wsapiVersion = webServiceVersion ?? DEFAULT_WSAPI_VERSION;
        }

        internal Uri AdhocUri
        {
            get { return new Uri(WebServiceUrl + "/adhoc.js"); }
        }

        /// <summary>
        /// The full WSAPI url
        /// </summary>
        public string WebServiceUrl
        {
            get { return Service.Server + "slm/webservice/" + wsapiVersion; }
        }

        #region Non Public

        static IEnumerable<string> DecodeArrayList(IEnumerable list)
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

        internal Uri FormatUpdateUri(string typePath, long objectId)
        {
            return
                new Uri(Service.Server.AbsoluteUri + "slm/webservice/" + wsapiVersion + "/" + typePath + "/" + objectId +
                        ".js");
        }


        private IEnumerable<object> GetCollection(object arr)
        {
            var list = arr as ArrayList;
            return list.Cast<object>();
        }

        #endregion


        #region Adhoc
        private DynamicJsonObject ProcessRequests(IEnumerable<AdhocRequest> requests)
        {
            Dictionary<string, object> dictionary = requests.ToDictionary<AdhocRequest, string, object>(r => r.Key,
                                                                                                   r => r.RequestUrl.Replace("\"", "\\\""));
            var obj = new DynamicJsonObject(dictionary);
            return serializer.Deserialize(Service.Post(AdhocUri, serializer.Serialize(obj), GetProcessedHeaders()));
        }

        private Dictionary<string, QueryResult> GatherSingleQuerySet(IEnumerable<AdhocRequest> requests)
        {
            DynamicJsonObject response = ProcessRequests(requests);
            if (response.Fields.Contains("OperationResult"))
            {
                var exception =
                    new Exception("Adhoc Query failed, Rally WSAPI Errors and Warnings included in exception data.");
                exception.Data.Add("Errors", response["OperationResult"].Errors);
                exception.Data.Add("Warnings", response["OperationResult"].Warnings);
                //TODO:// Fix this to just return errors
                throw exception;
            }
            return response.Fields.ToDictionary(i => i, i => new QueryResult((DynamicJsonObject)response[i]));
        }

        internal Dictionary<string, QueryResult> BatchQuery(IEnumerable<AdhocRequest> input)
        {
            List<AdhocRequest> requests = (from request in input select request.Clone()).ToList();
            Dictionary<string, QueryResult> results = GatherSingleQuerySet(requests);
            var subsequentQueries = new List<List<AdhocRequest>>();
            var list = new List<AdhocRequest>(requests);
            while (list.Count > 0)
            {
                list.Clear();
                foreach (AdhocRequest request in requests)
                {
                    int maxResultsAllowed = Math.Min(request.Limit, results[request.Key].TotalResultCount);
                    // Start has 1 for its lowest value.
                    var alreadyDownloadedItems = request.Start - 1 + request.PageSize;
                    var remainItemsToBeGathered = maxResultsAllowed - alreadyDownloadedItems;
                    if (remainItemsToBeGathered > 0)
                    {
                        var newRequest = request.Clone(request.Key, request.Start + request.PageSize);
                        request.Start += request.PageSize;

                        newRequest.PageSize = Math.Min(remainItemsToBeGathered, request.PageSize);
                        list.Add(newRequest);
                    }
                }

                if (list.Count > 0)
                {
                    subsequentQueries.Add(new List<AdhocRequest>(list));
                }
            }


            Parallel.ForEach(subsequentQueries, new ParallelOptions { MaxDegreeOfParallelism = MAX_THREADS_ALLOWED }, l =>
                    {
                        Dictionary<string, QueryResult> subsequentResults = GatherSingleQuerySet(l);

                        foreach (var r in l)
                        {
                            lock (results)
                            {
                                //TODO://KRM: Defect: Things may not come back in order!
                                List<object> currentResults =
                                    results[r.Key].Results.ToList();
                                currentResults.AddRange(
                                    subsequentResults[r.Key].Results);
                                results[r.Key].Results = currentResults;
                            }
                        }
                    });

            return results;
        }

        #endregion


        DynamicJsonObject MakeRequest(Uri uri)
        {
            return serializer.Deserialize(Service.Get(uri, GetProcessedHeaders()));
        }

        /// <summary>
        /// Perform a read against the WSAPI operation based
        /// on the data in the specified request
        /// </summary>
        /// <param name="request">The request configuration</param>
        /// <returns>The results of the read operation</returns>
        public QueryResult Query(Request request)
        {
            var response = MakeRequest(GetFullyQualifiedUri(request.RequestUrl));
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
                    var response1 = MakeRequest(GetFullyQualifiedUri(request1.RequestUrl));
                    lock(resultDictionary)
                    {
                        resultDictionary[request1.Start] = new QueryResult(response1["QueryResult"]);
                    }
                });

            var allResults = new List<object>(result.Results);
            foreach( var sortedResult in resultDictionary.ToList()
                .OrderBy(p=>p.Key))
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
            string type = null;
            try
            {
                type = Ref.GetTypeFromRef(aRef);
            }
            catch
            {
                //Handle case for things like user, subscription
                type = aRef.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            DynamicJsonObject wrappedReponse = MakeRequest(GetFullyQualifiedUri(aRef + "?fetch=" + string.Join(",", fetchedFields)));
            return type.Equals(wrappedReponse.Fields.FirstOrDefault(), StringComparison.CurrentCultureIgnoreCase) ? wrappedReponse[wrappedReponse.Fields.First()] : null;
        }

        /// <summary>
        /// Delete the object described by the specified type and object id.
        /// </summary>
        /// <param name="typePath">the type</param>
        /// <param name="oid">the object id</param>
        /// <returns>An OperationResult with information on the status of the request</returns>
        public OperationResult Delete(string workspaceRef, string typePath, long oid)
        {
            return Delete(workspaceRef, string.Format("/{0}/{1}", typePath, oid));
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
            dynamic response = serializer.Deserialize(Service.Delete(GetFullyQualifiedUri(aRef + workspaceClause), GetProcessedHeaders()));
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
            string postData = serializer.Serialize(data);
            DynamicJsonObject response = 
                serializer.Deserialize(Service.Post(FormatCreateUri(workspaceRef, typePath), postData, GetProcessedHeaders()));
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

        public DynamicJsonObject post(String relativeUri, DynamicJsonObject data)
        {
            Uri uri = new Uri(String.Format("{0}slm/webservice/{1}/{2}", Service.Server.AbsoluteUri, wsapiVersion, relativeUri));
            string postData = serializer.Serialize(data);
            return serializer.Deserialize(Service.Post(uri, postData, GetProcessedHeaders()));
        }

        /// <summary>
        /// Update the item described by the specified type and object id with
        /// the fields of the specified object
        /// </summary>
        /// <param name="typePath">the type of the item to be updated</param>
        /// <param name="oid">the object id of the item to be updated</param>
        /// <param name="obj">the object fields to update</param>
        /// <returns>An OperationResult describing the status of the request</returns>
        public OperationResult Update(string typePath, long oid, DynamicJsonObject obj)
        {
            var result = new OperationResult();
            var data = new DynamicJsonObject();
            data[typePath] = obj;
            string postData = serializer.Serialize(data);
            dynamic response =
                serializer.Deserialize(Service.Post(FormatUpdateUri(typePath, oid), postData, GetProcessedHeaders()));
            result.Errors.AddRange(DecodeArrayList(response.OperationResult.Errors));
            result.Warnings.AddRange(DecodeArrayList(response.OperationResult.Warnings));
            return result;
        }

        /// <summary>
        /// Get the allowed values for the specified type and attribute
        /// </summary>
        /// <param name="typePath">the type</param>
        /// <param name="attribute">the attribute to retireve allowed values for</param>
        /// <returns>The allowed values for the specified attribute</returns>
        public DynamicJsonObject GetAllowedAttributeValues(string typePath, string attribute)
        {
            return MakeRequest(GetFullyQualifiedUri(string.Format("/{0}/{1}/allowedValues.js", typePath, attribute)));
        }

        /// <summary>
        /// Get the attribute definitions for the specified type
        /// </summary>
        /// <param name="type">The type to get attributes for</param>
        /// <returns>The attribute definitions for the specified type</returns>
        public QueryResult GetAttributesByType(string type)
        {
            float apiVersion;
            float.TryParse(wsapiVersion, out apiVersion);

            if (wsapiVersion == DEFAULT_WSAPI_VERSION || apiVersion >= 1.25)
            {
                //In 1.25 forward we can just use the typedefs endpoint
                var attributesRequest = new Request("TypeDefinition");
                attributesRequest.Fetch = new List<string>() { "Attributes" };
                attributesRequest.Query = new Query("Name", RestApi.Query.Operator.Equals, type);
                var result = Query(attributesRequest);
                var attributeResult = result.Results.FirstOrDefault();
                if(attributeResult != null)
                {
                    var attributes = attributeResult["Attributes"] as ArrayList;
                    result.Results = attributes.Cast<object>().ToList<object>();
                    result.TotalResultCount = attributes.Count;
                }
                return result;
            }
            else
            {
                //Need to use adhoc to trace up the tree
                //pre 1.25 since type defs endpoint
                //didn't return inherited fields
                var requests = new List<AdhocRequest>
                                   {
                                       new AdhocRequest("TypeDefinition", "Type")
                                           {
                                               Fetch = new List<string>() {"Attributes"},
                                               Query = new Query("Name", RestApi.Query.Operator.Equals, type)
                                           },
                                       new PlaceholderRequest("Type/Parent", "Parent")
                                           {
                                               Fetch = new List<string>() {"Attributes"},
                                           },
                                       new PlaceholderRequest("Parent/Parent", "GrandParent")
                                           {
                                               Fetch = new List<string>() {"Attributes"},
                                           },
                                       new PlaceholderRequest("GrandParent/Parent", "GreatGrandParent")
                                           {
                                               Fetch = new List<string>() {"Attributes"},
                                           },
                                       new PlaceholderRequest("GreatGrandParent/Parent", "GreatGreatGrandParent")
                                           {
                                               Fetch = new List<string>() {"Attributes"},
                                           },
                                   };

                DynamicJsonObject response = ProcessRequests(requests);
                //This supports placeholder queries they return with a different signature.
                foreach (
                    string key in response.Dictionary.Keys.ToList().Where(key => response.Dictionary[key] is ArrayList)
                    )
                {
                    var list = ((ArrayList) response.Dictionary[key]);
                    if (list.Count > 0)
                    {
                        response.Dictionary[key] = ((ArrayList) response.Dictionary[key])[0];
                    }
                    else
                    {
                        response.Dictionary.Remove(key);
                    }
                }
                var result = new QueryResult(response["Type"]);
                ArrayList attributeCollection = response["Type"].Results.Count > 0
                                                    ? response["Type"].Results[0].Attributes
                                                    : new ArrayList();
                response.Dictionary.Remove("Type");
                foreach (dynamic val in response.Dictionary.Values)
                {
                    if (val.Equals("null")) continue;
                    List<DynamicJsonObject> jsonList =
                        (from IDictionary<string, object> attribute in val["Attributes"] as ArrayList
                         select new DynamicJsonObject(attribute)).ToList();
                    attributeCollection.AddRange(jsonList);
                }
                result.Results = attributeCollection.Cast<object>().ToList<object>();
                result.TotalResultCount = attributeCollection.Count;
                return result;
            }
        }
    }
}