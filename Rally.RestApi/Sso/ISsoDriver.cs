using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Sso
{
	/// <summary>
	/// An interface used for SSO Authentication.
	/// </summary>
	public interface ISsoDriver
	{
		/// <summary>
		/// Is SSO authorized?
		/// </summary>
		bool IsSsoAuthorized { get; }
		/// <summary>
		/// The event that is triggered when SSO is completed.
		/// </summary>
		event SsoResults SsoResults;
		/// <summary>
		/// Shows the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The Uri that the user was redirected to in order to perform their SSO authentication.</param>
		void ShowSsoPage(Uri ssoUrl);
	}

	/// <summary>
	/// A delegate to indicate that SSO has completed.
	/// </summary>
	/// <param name="success">Was SSO authentication completed successfully?</param>
	/// <param name="zSessionID">The zSessionID that was returned from Rally.</param>
	public delegate void SsoResults(bool success, string zSessionID);
}
