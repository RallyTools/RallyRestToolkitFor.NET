using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Sso
{
	/// <summary>
	/// An SSO driver that states that SSO is not allowed.
	/// </summary>
	internal class SsoNotAllowedDriver : ISsoDriver
	{
		/// <summary>
		/// Is SSO authorized?
		/// </summary>
		public bool IsSsoAuthorized { get { return false; } }
		/// <summary>
		/// The event that is triggered when SSO is completed.
		/// </summary>
		public event SsoResults SsoResults;
		/// <summary>
		/// Shows the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The URL that the user was redirected to in order to perform their SSO authentication.</param>
		public void ShowSsoPage(Uri ssoUrl)
		{
		}

		private void FireEventDummy()
		{
			SsoResults.Invoke(false, null);
		}
	}
}
