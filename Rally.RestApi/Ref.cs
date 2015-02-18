using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rally.RestApi
{
	/// <summary>
	/// The Ref class contains a set of utility methods
	/// for working with refs.
	/// </summary>
	public static class Ref
	{
		#region Regexes
		private static List<Regex> regexes = new List<Regex>() {

            //dynatype collection ref (/portfolioitem/feature/1234/children)
            new Regex (".*/(\\w{2,}/\\w+)/(\\d+/\\w+)(?:\\.js)*(?:\\?.*)*$"),
            
            //dynatype ref (/portfolioitem/feature/1234)
            new Regex (".*/(\\w{2,}/\\w+)/(\\d+)(?:\\.js)*(?:\\?.*)*$"),

            //collection ref (/defect/1234/tasks)
            new Regex (".*/(\\w+/-?\\d+)/(\\w+)(?:\\.js)*(?:\\?.*)*$"),

            //basic ref (/defect/1234)
            new Regex (".*/(\\w+)/(-?\\d+)(?:\\.js)*(?:\\?.*)*$"),

            //permission ref (/workspacepermission/123u456w1)
            new Regex (".*/(\\w+)/(\\d+u\\d+[pw]\\d+)(?:\\.js)*(?:\\?.*)*$"),
        };
		#endregion

		#region IsRef
		/// <summary>
		/// Determine whether the specified string is a reference
		/// </summary>
		/// <param name="reference">the ref to test</param>
		/// <returns>true if a ref, false otherwise</returns>
		/// <example>
		/// <code language="C#">
		/// bool isValid;
		/// isValid = Ref.IsRef("http://rally1.rallydev.com/slm/webservice/1.32/defect/1234.js");
		/// // isValid is true
		/// 
		/// 
		/// isValid = Ref.IsRef("https://rally1.rallydev.com/slm/webservice/1.32/defect/abc.js");
		/// // isValid is false
		/// </code>
		/// </example>
		public static bool IsRef(string reference)
		{
			return GetMatch(reference) != null;
		}
		#endregion

		#region GetRelativeRef
		/// <summary>
		/// Get a relative ref from the specified ref.
		/// All server information will be stripped before being returned.
		/// </summary>
		/// <param name="reference">The absolute ref to be made relative</param>
		/// <returns>The relative version of the specified absolute ref</returns>
		/// <example>
		/// <code language="C#">
		/// string fullyQualifiedRef = "https://rally1.rallydev.com/slm/webservice/v2.0/portfolioitem/feature/1234";
		/// string relativeRef = Ref.GetRelativeRef(fullyQualifiedRef);
		/// 
		/// // returns "/portfolioitem/feature/1234"
		/// </code>
		/// </example>
		public static string GetRelativeRef(string reference)
		{
			Match m = GetMatch(reference);
			if (m != null)
				return String.Format("/{0}/{1}", m.Groups[1].Value, m.Groups[2].Value);
			else
				return null;
		}
		#endregion

		#region GetTypeFromRef
		/// <summary>
		/// Get the type from the specified ref
		/// </summary>
		/// <param name="reference">The ref to get the type from</param>
		/// <returns>The type of the specified ref</returns>
		/// <example>
		/// <code language="C#">
		/// string fullyQualifiedRef = "https://rally1.rallydev.com/slm/webservice/v2.0/user/1234";
		/// string relativeRef = Ref.GetTypeFromRef(fullyQualifiedRef);
		/// 
		/// // returns "user"
		/// </code>
		/// </example>
		public static string GetTypeFromRef(string reference)
		{
			Match m = GetMatch(reference);
			if (m != null)
				return m.Groups[1].Value;
			else
				return null;
		}
		#endregion

		#region GetOidFromRef
		/// <summary>
		/// Get the object id from the specified ref
		/// </summary>
		/// <param name="reference">The ref to get the object id from</param>
		/// <returns>The object id of the specified ref</returns>
		/// <example>
		/// <code language="C#">
		/// string fullyQualifiedRef = "https://rally1.rallydev.com/slm/webservice/v2.0/user/1234";
		/// string relativeRef = Ref.GetOidFromRef(fullyQualifiedRef);
		/// 
		/// // returns "1234"
		/// </code>
		/// </example>
		public static string GetOidFromRef(string reference)
		{
			Match m = GetMatch(reference);
			if (m != null)
				return m.Groups[2].Value;
			else
				return null;
		}
		#endregion

		#region Helper: GetMatch
		private static Match GetMatch(string reference)
		{
			foreach (Regex r in regexes)
			{
				if (r.IsMatch(reference ?? ""))
				{
					return r.Match(reference);
				}
			}

			return null;
		}
		#endregion
	}
}
