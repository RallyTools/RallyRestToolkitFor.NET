using Rally.RestApi.Json;
using System.Collections.Generic;
using System.Linq;

namespace Rally.RestApi.Response
{
	/// <summary>
	/// This class represents the result of
	/// an operation against the WSAPI.
	/// </summary>
	public class OperationResult
	{
		/// <summary>
		/// The object created
		/// </summary>
		public DynamicJsonObject Object { get; set; }

		/// <summary>
		/// A list of any errors that occurred during the request
		/// </summary>
		public List<string> Errors { get; set; }

		/// <summary>
		/// A list of any warnings that occurred during the request
		/// </summary>
		public List<string> Warnings { get; set; }

		/// <summary>
		/// Whether the request was successful or not
		/// Returns true if Errors is empty
		/// </summary>
		public bool Success { get { return Errors.Count() == 0; } }

		/// <summary>
		/// Create a new empty OperationResult
		/// </summary>
		public OperationResult()
		{
			Errors = new List<string>();
			Warnings = new List<string>();
		}
	}
}
