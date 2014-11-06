using Rally.RestApi.Json;
using System.Collections.Generic;

namespace Rally.RestApi.Response
{
	/// <summary>
	/// Object returned from a create opreration
	/// </summary>
	public class CreateResult : OperationResult
	{
		/// <summary>
		/// Create a new empty CreateResult
		/// </summary>
		public CreateResult()
		{
			Errors = new List<string>();
			Warnings = new List<string>();
		}

		/// <summary>
		/// The ref of the created item
		/// </summary>
		public string Reference { get; set; }
	}
}
