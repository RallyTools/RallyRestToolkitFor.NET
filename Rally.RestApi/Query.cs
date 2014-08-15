﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;

namespace Rally.RestApi
{
	/// <summary>
	/// This class represents a filter
	/// for query operations.
	/// </summary>
	[Serializable]
	public class Query
	{
		#region Enumeration: Operator
		/// <summary>
		/// The available query operators
		/// </summary>
		public enum Operator
		{
			/// <summary>
			/// =
			/// </summary>
			Equals,
			/// <summary>
			/// !=
			/// </summary>
			DoesNotEqual,
			/// <summary>
			/// contains
			/// </summary>
			Contains,
			/// <summary>
			/// !contains
			/// </summary>
			DoesNotContain,
			/// <summary>
			/// <![CDATA[<]]>
			/// </summary>
			LessThan,
			/// <summary>
			/// <![CDATA[<=]]>
			/// </summary>
			LessThanOrEqualTo,
			/// <summary>
			/// <![CDATA[>]]>
			/// </summary>
			GreaterThan,
			/// <summary>
			/// <![CDATA[>=]]>
			/// </summary>
			GreaterThanOrEqualTo
		}
		#endregion

		#region Enumeration: ClauseOperator
		/// <summary>
		/// An enumeration of the available operators
		/// to join query clauses
		/// </summary>
		protected enum ClauseOperator
		{
			/// <summary>
			/// AND
			/// </summary>
			And,

			/// <summary>
			/// OR
			/// </summary>
			Or
		}
		#endregion

		#region Properties and Fields
		static readonly Dictionary<Operator, string> OpMap = new Dictionary<Operator, string>()
        {
            {Operator.Equals, "="},
            {Operator.Contains, "contains"},
            {Operator.DoesNotContain, "!contains"},
            {Operator.LessThan, "<"},
            {Operator.LessThanOrEqualTo, "<="},
            {Operator.GreaterThan, ">"},
            {Operator.GreaterThanOrEqualTo, ">="},
            {Operator.DoesNotEqual, "!="}
        };
		private string queryClause;
		/// <summary>
		/// The attribute to filter by
		/// </summary>
		public string Attribute { get; set; }
		/// <summary>
		/// The filter operator
		/// </summary>
		public Operator QueryOperator { get; set; }
		/// <summary>
		/// The value to be filtered on
		/// </summary>
		public string Value { get; set; }
		#endregion

		#region Calculated Properties
		/// <summary>
		/// Get the string representation of this query
		/// </summary>
		public string QueryClause
		{
			get
			{
				if (queryClause != null)
				{
					return queryClause;
				}
				const char quote = '"';
				// Quote the value
				// Removed due to issues of double encoding, left for reference in case something breaks.
				// var outValue = quote + Value.Replace("%", "%25").Replace("&", "%26").Replace("#", "%23").Replace("\"", "%22").Replace("+", "%2B") + quote;
				string outValue;
				if (Value == null)
					outValue = "null";
				else
					outValue = quote + Value + quote;
				return string.Format("({0} {1} {2})", Attribute, GetOperator(QueryOperator), outValue);
			}

			set
			{
				queryClause = value;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Create a new empty query
		/// </summary>
		public Query()
		{
		}
		/// <summary>
		/// Create a new query built from the specified
		/// attribute, operator and value
		/// </summary>
		/// <param name="attribute">The attribute to be filtered by</param>
		/// <param name="op">The filter operator</param>
		/// <param name="value">The value to be filtered on</param>
		public Query(string attribute, Operator op, string value)
		{
			Attribute = attribute;
			QueryOperator = op;
			Value = value;
		}
		/// <summary>
		/// Create a new query from the specified string.
		/// </summary>
		/// <param name="queryClause">The query string</param>
		public Query(string queryClause)
		{
			this.queryClause = queryClause;
		}
		#endregion

		#region Join
		/// <summary>
		/// Join the two specified queries with the specified operator
		/// </summary>
		/// <param name="a">The first query to be joined</param>
		/// <param name="b">The second query to be joined</param>
		/// <param name="op">The operator</param>
		/// <returns>The joined query</returns>
		protected Query Join(Query a, Query b, ClauseOperator op)
		{
			return new Query(string.Format("({0} {1} {2})", a.QueryClause, op.ToString().ToUpper(), b.QueryClause));
		}
		#endregion

		#region GetOperator
		/// <summary>
		/// Get the string version of the specified operator
		/// </summary>
		/// <param name="op">The operator to translate</param>
		/// <returns>The string version of the specified operator</returns>
		public static string GetOperator(Operator op)
		{
			return OpMap[op];
		}
		/// <summary>
		/// Parse the specified string operator into
		/// a value of the Operator enum.
		/// </summary>
		/// <param name="op">The operator to translate</param>
		/// <returns>The matching Operator value</returns>
		public static Operator GetOperator(string op)
		{
			return OpMap.Single(k => k.Value == op).Key;
		}
		#endregion

		#region And
		/// <summary>
		/// Join the specified query to this query
		/// using the AND operator.
		/// </summary>
		/// <param name="q">The query to be joined</param>
		/// <returns>The joined query</returns>
		public Query And(Query q)
		{
			return Join(this, q, ClauseOperator.And);
		}

		/// <summary>
		/// Join the specified queries using the AND operator
		/// </summary>
		/// <param name="queries">The queries to be joined</param>
		/// <returns>The joined query</returns>
		public static Query And(params Query[] queries)
		{
			Query result = null;
			foreach (Query q in queries)
			{
				if (result == null)
					result = q;
				else
					result = result.And(q);
			}

			return result;
		}
		#endregion

		#region Or
		/// <summary>
		/// Join the specified query to this query
		/// using the OR operator.
		/// </summary>
		/// <param name="q">The query to be joined</param>
		/// <returns>The joined query</returns>
		public Query Or(Query q)
		{
			return Join(this, q, ClauseOperator.Or);
		}

		/// <summary>
		/// Join the specified queries using the OR operator
		/// </summary>
		/// <param name="queries">The queries to be joined</param>
		/// <returns>The joined query</returns>
		public static Query Or(params Query[] queries)
		{
			Query result = null;
			foreach (Query q in queries)
			{
				if (result == null)
					result = q;
				else
					result = result.Or(q);
			}

			return result;
		}
		#endregion

		#region Parse
		/// <summary>
		/// Parse the specified string into a query
		/// </summary>
		/// <param name="query">The query string to be parsed</param>
		/// <returns>A query object, or null if the string could not be parsed</returns>
		public static Query Parse(string query)
		{
			try
			{
				Regex r = new Regex("^\\((?<attribute>\\w+) (?<op>.+) (?<value>.+)\\)$");
				Match m = r.Match(query);
				return new Query(m.Groups["attribute"].Value,
								OpMap.Keys.Single(k => OpMap[k] == m.Groups["op"].Value),
								m.Groups["value"].Value);
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region TryParseQueryOperator
		/// <summary>
		/// Tries to get the operator based upon the operator query string
		/// </summary>
		/// <param name="operatorString">The string to parse.</param>
		/// <param name="queryOperator">The operator that was found.</param>
		/// <returns>If the parsing was successful. If not, the operator is set to Equals.</returns>
		public static bool TryParseQueryOperator(string operatorString, out Operator queryOperator)
		{
			queryOperator = Operator.Equals;
			foreach (Operator key in OpMap.Keys)
			{
				if (OpMap[key].Equals(operatorString, StringComparison.InvariantCultureIgnoreCase))
				{
					queryOperator = key;
					return true;
				}
			}

			return false;
		}
		#endregion

		#region ToString (Override as QueryClause)
		/// <summary>
		/// Same as QueryClause
		/// </summary>
		/// <returns>The string representation of the query</returns>
		public override string ToString()
		{
			return QueryClause;
		}
		#endregion
	}
}
