using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Exceptions
{
	/// <summary>
	/// An exception indicating that Rally is temporarily off-line and returned an HTML error page.
	/// </summary>
	public class RallyUnavailableException : Exception
	{
		/// <summary>
		/// The error message from Rally
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		/// <param name="errorMessage">The HTML error message that was returned from Rally.</param>
		internal RallyUnavailableException(Exception innerException, string errorMessage)
			: base("Rally Unavailable", innerException)
		{
			ErrorMessage = errorMessage;
		}
	}
}
