using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Rally.RestApi.Response
{
	/// <summary>
	/// This class represents the result of a query operation
	/// </summary>
	public class CacheableQueryResult : QueryResult
	{
		/// <summary>
		/// Is this result a cached result? If not, it contains updated data.
		/// </summary>
		public bool IsCachedResult { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public CacheableQueryResult(DynamicJsonObject obj, bool isCachedResult)
			: base(obj)
		{
			IsCachedResult = isCachedResult;
		}
	}
}
