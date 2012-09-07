using System;

namespace Rally.RestApi
{
    [Serializable]
    internal class AdhocRequest : Request
    {
        public string Key { get; set; }


        /// <summary>
        /// Builds a request
        /// </summary>
        /// <param name="typeName">The Rally artifact artifactName the query should return</param>
        /// <param name="key">The key you that the return data will be stored under.</param>
        public AdhocRequest(string typeName, string key)
            : base(typeName)
        {
            Key = key;
        }

        internal override string RequestUrl
        {
            get { return BuildQueryString(); }
        }

        protected internal override string EndpointName
        {
            get { return ArtifactName; }
        }

        public new AdhocRequest Clone()
        {
            var request = new AdhocRequest(ArtifactName, Key) { Limit = Limit };
            foreach (var dictionaryKey in Parameters.Keys)
            {
                request.Parameters[dictionaryKey] = Parameters[dictionaryKey];
            }
            return request;
        }

        internal AdhocRequest Clone(string key, int pageStart)
        {
            var request = Clone();
            request.Key = key;
            request.Start = pageStart;
            return request;
        }
    }
}
