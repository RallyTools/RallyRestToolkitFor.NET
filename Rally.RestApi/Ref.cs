using System;

namespace Rally.RestApi
{
    /// <summary>
    /// The Ref class contains a set of utility methods
    /// for working with refs.
    /// </summary>
    public static class Ref
    {
        /// <summary>
        /// Get a relative ref from the specified ref.
        /// All server information will be stripped before being returned.
        /// </summary>
        /// <param name="reference">The absolute ref to be made relative</param>
        /// <returns>The relative version of the specified absolute ref</returns>
        public static string GetRelativeRef(string reference)
        {
            var tokens = TokenizeRef(reference);
            var relRef = "/" + tokens[tokens.Length - 2] + "/" + (tokens[tokens.Length - 1].Split('.')[0]);
            return relRef;
        }

        private static string[] TokenizeRef(string reference)
        {
            var tokens = reference.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                throw new ArgumentException("Must be a valid reference", "reference");
            }
            return tokens;
        }

        /// <summary>
        /// Get the type from the specified ref
        /// </summary>
        /// <param name="reference">The ref to get the type from</param>
        /// <returns>The type of the specified ref</returns>
        public static string GetTypeFromRef(string reference)
        {
            var tokens = TokenizeRef(reference);
            return tokens[tokens.Length - 2];
        }

        /// <summary>
        /// Get the object id from the specified ref
        /// </summary>
        /// <param name="reference">The ref to get the object id from</param>
        /// <returns>The object id of the specified ref</returns>
        public static long GetOidFromRef(string reference)
        {
            var tokens = TokenizeRef(reference);
            var oidToken = tokens[tokens.Length - 1];
            return long.Parse(oidToken.Replace(".js", ""));
        } 
    }
}
