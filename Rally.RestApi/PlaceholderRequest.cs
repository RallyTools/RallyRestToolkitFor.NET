using System;

namespace Rally.RestApi
{

    /// <summary>
    /// Represents a request to be sent to Rally
    /// </summary>
    [Serializable]
    internal class PlaceholderRequest : AdhocRequest
    {
        public string PlaceHolder { get; private set; }
        /// <summary>
        /// Builds a request
        /// </summary>
        /// <param name="placeholder">The </param>
        /// <param name="key">The key you that the return data will be stored under.</param>
        public PlaceholderRequest(string placeholder, string key)
            : base(placeholder, key)
        {
            PlaceHolder = placeholder;
        }

        internal override string RequestUrl
        {
            get
            {
                if (Fetch != null && Fetch.Count > 0)
                {
                    return string.Format("${{{0}?fetch={1}}}", PlaceHolder, string.Join(",", Fetch));
                }
                else
                {
                    return "${" + PlaceHolder + "?fetch=true}";
                }
            }
        }
    }
}
