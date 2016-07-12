using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rally.RestApi
{
	/// <summary>
	/// Diagnostic trace settings
	/// </summary>
	[Flags]
	public enum TraceFieldEnum
	{
		/// <summary>
		/// No trace
		/// </summary>
		None = 0,

		/// <summary>
		/// Includes Request/Response Data
		/// </summary>
		Data = 1,

		/// <summary>
		/// Include Request/Response Headers
		/// </summary>
		Headers = 2,

		/// <summary>
		/// Include Before/After Cookies
		/// </summary>
		Cookies = 4
	}

	/// <summary>
	/// Helper class for logging diagnostic trace messages
	/// </summary>
	public static class TraceHelper
	{
		/// <summary>
		/// Variable controlling level of Trace output
		/// </summary>
		public static TraceFieldEnum TraceFields { get; set; }

		/// <summary>
		/// Log a Trace message
		/// </summary>
		public static void TraceMessage(string format, params object[] args)
		{
			if (TraceHelper.TraceFields > 0)
			{
				Trace.TraceInformation(format + "\r\n\r\n", args);
			}
		}

		/// <summary>
		/// Log a Http Trace message
		/// </summary>
		public static void TraceHttpMessage(string action, DateTime startTime, Uri target, string requestHeaders, object responseData, string responseHeaders)
		{
			TraceHttpMessage(action, startTime, target, null, requestHeaders, null, responseData, responseHeaders, null);
		}

		/// <summary>
		/// Log a Http Trace message
		/// </summary>
		public static void TraceHttpMessage(string action, DateTime startTime, Uri target, string requestHeaders, string cookiesBefore, object responseData, string responseHeaders, string cookiesAfter)
		{
			TraceHttpMessage(action, startTime, target, null, requestHeaders, cookiesBefore, responseData, responseHeaders, cookiesAfter);
		}

		/// <summary>
		/// Log a Http Trace message
		/// </summary>
		public static void TraceHttpMessage(string action, DateTime startTime, Uri target, object requestData, string requestHeaders, string cookiesBefore, object responseData, string responseHeaders, string cookiesAfter)
		{
			if (TraceHelper.TraceFields > 0)
			{
				string traceRequestData = "", traceResponseData = "", traceRequestHeaders = "", traceResponseHeaders = "", traceCookiesBefore = "", traceCookiesAfter = "";

				var traceSummary = string.Format("{0} ({1}):\r\n{2}", action, DateTime.Now.Subtract(startTime).ToString(), target.ToString());

				// Include data
				if (TraceHelper.TraceFields.HasFlag(TraceFieldEnum.Data))
				{
					traceRequestData = requestData == null ? "" : string.Format("\r\nRequest Data:\r\n{0}\r\n", requestData);
					traceResponseData = string.Format("\r\nResponse Data\r\n{0}", responseData);
				}

				// Include headers
				if (TraceHelper.TraceFields.HasFlag(TraceFieldEnum.Headers))
				{
					traceRequestHeaders = string.Format("\r\nRequest Headers:\r\n{0}", requestHeaders);
					traceResponseHeaders = string.Format("\r\nResponse Headers:\r\n{0}", responseHeaders);
				}

				// Include cookies
				if (TraceHelper.TraceFields.HasFlag(TraceFieldEnum.Cookies))
				{
					traceCookiesBefore = string.Format("\r\nCookies Before:\r\n{0}", cookiesBefore);
					traceCookiesAfter = string.Format("\r\nCookies After:\r\n{0}", cookiesAfter);
				}

				// Log the trace information
				Trace.TraceInformation(String.Concat(
					traceSummary,
					traceRequestHeaders,
					traceCookiesBefore,
					traceRequestData,
					traceResponseHeaders,
					traceCookiesAfter,
					traceResponseData,
					"\r\n\r\n"));
			}
		}
	}
}