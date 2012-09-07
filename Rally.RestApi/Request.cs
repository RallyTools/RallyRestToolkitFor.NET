using System;
using System.Collections.Generic;
using System.Linq;

namespace Rally.RestApi
{

    /// <summary>
    /// Represents a request to be sent to Rally
    /// </summary>
    [Serializable]
    public class Request
    {
        /// <summary>
        /// The maximum page size (200).
        /// </summary>
        public const int MAX_PAGE_SIZE = 200;

        internal Dictionary<string, dynamic> Parameters { get; private set; }
        
        /// <summary>
        /// Create a new Request with the specified artifact type
        /// </summary>
        /// <param name="artifactName">The Rally artifact type being requested</param>
        public Request(string artifactName)
            : this()
        {
            ArtifactName = artifactName;
        }

        /// <summary>
        /// Create a new empty Request.
        /// </summary>
        public Request()
        {
            Parameters = new Dictionary<string, dynamic>();
            Fetch = new List<string>();
            PageSize = MAX_PAGE_SIZE;
            Start = 1;
            Limit = PageSize;
        }


        /// <summary>
        /// A filter query to be applied to results before being returned
        /// </summary>
        public Query Query
        {
            get { return Parameters.ContainsKey("query") ? Parameters["query"] : null; }
            set { Parameters["query"] = value; }
        }

        /// <summary>
        /// An upper bound on the total results to be returned.
        /// </summary>
        public int Limit
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the artifact that will be queried
        /// </summary>
        public string ArtifactName { get; set; }

        /// <summary>
        /// Page size for results. Must be between 1 and MAX_PAGE_SIZE, default is MAX_PAGE_SIZE. 
        /// </summary>
        public int PageSize
        {
            get;
            set;
        }
        /// <summary>
        /// Default is the user's default from Rally. In addition to the specified project, include projects above the specified one. 
        /// </summary>
        public bool? ProjectScopeUp
        {
            get { return Parameters.ContainsKey("projectScopeUp") ? Parameters["projectScopeUp"] : null; }
            set { Parameters["projectScopeUp"] = value; }
        }
        /// <summary>
        /// Default is the user's default from Rally. In addition to the specified project, include child projects below the specified one. 
        /// </summary>
        public bool? ProjectScopeDown
        {
            get { return Parameters.ContainsKey("projectScopeDown") ? Parameters["projectScopeDown"] : null; }
            set { Parameters["projectScopeDown"] = value; }
        }
        /// <summary>
        /// Start index (1-based) for queries. The default is 1. 
        /// </summary>
        public int Start
        {
            get { return Parameters.ContainsKey("start") ? Parameters["start"] : 1; }
            set { Parameters["start"] = value; }
        }
        /// <summary>
        /// The ref for the workspace that you want the results from
        /// <example>
        /// /workspace/12345
        /// </example>
        /// </summary>
        public string Workspace
        {
            get { return Parameters.ContainsKey("workspace") ? Parameters["workspace"] : null; }
            set { Parameters["workspace"] = value; }
        }
        /// <summary>
        /// The ref for the project that you want the results from
        /// <example>
        /// /project/12345
        /// </example>
        /// </summary>
        public string Project
        {
            get { return Parameters.ContainsKey("project") ? Parameters["project"] : null; }
            set { Parameters["project"] = value; }
        }

        /// <summary>
        /// A list of attributes to be returned in the result set.
        /// If null or empty true will be used.
        /// </summary>
        public List<string> Fetch
        {
            get;
            set;
        }

        /// <summary>
        ///  A sort string. 
        ///  <example>ObjectId Desc</example>
        ///  <example>FormattedId</example>
        /// </summary>
        public string Order
        {
            get { return Parameters.ContainsKey("order") ? Parameters["order"] : null; }
            set { Parameters["order"] = value; }
        }

        /// <summary>
        /// Create a query string from this request.
        /// </summary>
        /// <param name="extension">The extension to use for the type (default = "")</param>
        /// <returns>A query string representation of this request</returns>
        protected string BuildQueryString(string extension = "")
        {
            int pageSize = Math.Min(Math.Min(MAX_PAGE_SIZE, PageSize), Limit);
            var list = new List<string>(new[] {
                    "pagesize=" + pageSize
                });
            var fetch = Fetch.Count == 0 ? new List<string>() { "true" } : new List<string>(Fetch);

            var tmpParameters = new Dictionary<string, dynamic>(Parameters);

            if (tmpParameters.Keys.Contains("order"))
            {
                string orderString = tmpParameters["order"].ToString();
                List<string> orderList = orderString.Split(',').ToList();

                if (!orderList.Contains("ObjectID") && !orderList.Contains("ObjectID desc"))
                {
                    // Add 'ObjectID' to an existing order clause to workaround server-side WSAPI bug
                    orderList.Add("ObjectID");
                    tmpParameters["order"] = string.Join(",", orderList);
                }
            }
            else
            {
                // Add order on 'ObjectID' clause to workaround server-side WSAPI bug
                tmpParameters["order"] = "ObjectID";
            }

            foreach (var key in from k in tmpParameters.Keys orderby k select k)
            {
                if (tmpParameters[key] == null)
                    continue;
                if (tmpParameters[key] is bool)
                {
                    list.Add(key + "=" + tmpParameters[key].ToString().ToLower());
                }
                else
                {
                    string value = tmpParameters[key].ToString();
                    list.Add(key + "=" + value);
                }
            }

            list.Add("fetch=" + string.Join(",", fetch));
            return "/" + EndpointName + extension + "?" + string.Join("&", list.ToArray());
        }

        protected internal virtual string EndpointName
        {
            get
            {
                switch(ArtifactName.ToLower())
                {
                    case "user":
                    case "subscription":
                        return ArtifactName.ToLower() + "s"; //special case for user/subscription endpoints
                }

                return ArtifactName.ToLower();
            }
        }

        internal virtual string RequestUrl
        {
            get { return BuildQueryString(".js"); }
        }

        /// <summary>
        /// Perform a deep clone of this request and all its parameters.
        /// </summary>
        /// <returns>The clone request</returns>
        public Request Clone()
        {
            var request = new Request(ArtifactName) { Limit = Limit };
            foreach (var dictionaryKey in Parameters.Keys)
            {
                request.Parameters[dictionaryKey] = Parameters[dictionaryKey];
            }
            request.Fetch = new List<string>(this.Fetch);
            return request;
        }

        internal Request Clone(int pageStart)
        {
            var request = Clone();
            request.Start = pageStart;
            return request;
        }

    }
}
