using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Rally.RestApi.Json;

namespace Rally.RestApi.Response
{
	/// <summary>
	/// This class represents the result
	/// of a query operation
	/// </summary>
	public class QueryResult : OperationResult
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="obj">The data that was returned for the query against Rally.</param>
		public QueryResult(DynamicJsonObject obj)
		{
			Errors = GetCollection<string>(obj["Errors"]);
			Warnings = GetCollection<string>(obj["Warnings"]);
			if (obj.Dictionary.ContainsKey("TotalResultCount"))
			{
				TotalResultCount = (int)obj["TotalResultCount"];
				StartIndex = (int)obj["StartIndex"];
				Results = GetCollection<dynamic>(obj["Results"]);
			}
			else
			{
				TotalResultCount = 1;
				Results = GetCollection<object>(new ArrayList() { obj });
				obj.Dictionary.Remove("Errors");
				obj.Dictionary.Remove("Warnings");
			}
		}

		/// <summary>
		/// The total number of results
		/// </summary>
		public int TotalResultCount { get; internal set; }

		/// <summary>
		/// The start index of this result set
		/// </summary>
		public int StartIndex { get; internal set; }

		/// <summary>
		/// The results of the query
		/// </summary>
		public IEnumerable<dynamic> Results { get; internal set; }

		private static IEnumerable<T> GetCollection<T>(object arr)
		{
			var list = arr as ArrayList;
			return (from dynamic i in list select (T)i).ToList();
		}
	}
}
