using System;
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

        static readonly Dictionary<Operator, string> OpMap = new Dictionary<Operator, string>()
        {
            {Operator.Equals, "="},
            {Operator.Contains, "contains"},
            {Operator.LessThan, "<"},
            {Operator.LessThanOrEqualTo, "<="},
            {Operator.GreaterThan, ">"},
            {Operator.GreaterThanOrEqualTo, ">="},
            {Operator.DoesNotEqual, "!="}
        };

        private string queryClause;

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
        /// Join the specified queries using the AND operator
        /// </summary>
        /// <param name="queries">The queries to be joined</param>
        /// <returns>The joined query</returns>
        public static Query And(params Query[] queries)
        {
            Query result = null;
            foreach (Query q in queries)
            {
                result = result == null ? q : result.And(q);
            }

            return result;
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
                result = result == null ? q : result.Or(q);
            }

            return result;
        }

        /// <summary>
        /// The attribute to filter by
        /// </summary>
        public string Attribute
        {
            get;
            set;
        }

        /// <summary>
        /// The filter operator
        /// </summary>
        public Operator QueryOperator
        {
            get;
            set;
        }

        /// <summary>
        /// The value to be filtered on
        /// </summary>
        public string Value
        {
            get;
            set;
        }

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
                var outValue = quote + Value.Replace("%", "%25").Replace("&", "%26").Replace("#", "%23").Replace("\"", "%22").Replace("+", "%2B") + quote;
                return string.Format("({0} {1} {2})", Attribute, GetOperator(QueryOperator), outValue);
            }

            set
            {
                queryClause = value;
            }
        }

        /// <summary>
        /// Same as QueryClause
        /// </summary>
        /// <returns>The string representation of the query</returns>
        public override string ToString()
        {
            return QueryClause;
        }

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
    }
}
