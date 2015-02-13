using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Exceptions
{
	/// <summary>
	/// An exception that indicates that we failed to deserialize JSON returned from Rally.
	/// </summary>
	public class RallyFailedToDeserializeJson : Exception
	{
		/// <summary>
		/// The JSON data that was returned from Rally.
		/// </summary>
		public string JsonData { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		/// <param name="jsonData">The JSON data that was returned from Rally.</param>
		internal RallyFailedToDeserializeJson(Exception innerException, string jsonData)
			: base("Failed to Deserialize JSON", innerException)
		{
			JsonData = jsonData;
		}
	}
}
