using mshtml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rally.RestApi.UiForWinforms
{
	internal partial class SsoWindow : Form
	{
		bool ssoReported = false;
		RestApiAuthMgrWinforms authMgr;
		/// <summary>
		/// Is SSO authorized?
		/// </summary>
		public bool IsSsoAuthorized { get { return true; } }

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public SsoWindow()
		{
			InitializeComponent();
			browser.DocumentCompleted += browser_DocumentCompleted;
		}
		#endregion

		#region ShowSsoPage
		/// <summary>
		/// Shows the specified SSO URL to the user.
		/// </summary>
		internal void ShowSsoPage(RestApiAuthMgrWinforms authMgr, Uri ssoUrl)
		{
			if (authMgr == null)
				throw new ArgumentNullException("authMgr", "You must provide an authorization manager.");

			if (ssoUrl == null)
				throw new ArgumentNullException("ssoUrl", "You must provide a URL for completing SSO authentication.");

			this.authMgr = authMgr;
			browser.Url = ssoUrl;
			Show();
		}
		#endregion

		#region browser_DocumentCompleted

		private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			try
			{
				CookieContainer cookiejar = GetUriCookieContainer(browser.Url);
				if (cookiejar == null)
					return;

				CookieCollection cookieCollection = cookiejar.GetCookies(browser.Url);
				if (cookieCollection == null)
					return;

				foreach (Cookie currentCookie in cookieCollection)
				{
					if (currentCookie.Name.Equals(RallyRestApi.ZSessionID, StringComparison.InvariantCultureIgnoreCase))
					{
						WindowState = FormWindowState.Minimized;

						string rallyServer = browser.Url.GetLeftPart(UriPartial.Authority);
						authMgr.ReportSsoResultsToMgr(true, rallyServer, currentCookie.Value);
						ssoReported = true;
						Close();
					}
				}
			}
			catch
			{ }
		}
		#endregion

		#region InternetGetCookieEx
		[System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true)]
		public static extern bool InternetGetCookieEx(
				string url,
				string cookieName,
				StringBuilder cookieData,
				ref int size,
				Int32 dwFlags,
				IntPtr lpReserved);

		private const Int32 InternetCookieHttponly = 0x2000;
		#endregion

		#region GetUriCookieContainer
		/// <summary>
		/// Gets the URI cookie container.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static CookieContainer GetUriCookieContainer(Uri uri)
		{
			CookieContainer cookies = null;
			// Determine the size of the cookie
			int datasize = 8192 * 16;
			StringBuilder cookieData = new StringBuilder(datasize);
			if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
			{
				if (datasize < 0)
					return null;
				// Allocate stringbuilder large enough to hold the cookie
				cookieData = new StringBuilder(datasize);
				if (!InternetGetCookieEx(
						uri.ToString(),
						null, cookieData,
						ref datasize,
						InternetCookieHttponly,
						IntPtr.Zero))
					return null;
			}
			if (cookieData.Length > 0)
			{
				cookies = new CookieContainer();
				cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
			}
			return cookies;
		}
		#endregion

		#region OnClosed
		/// <summary>
		/// Override of OnClosed to be able to indicate that the user closed the window prior to completing SSO.
		/// </summary>
		protected override void OnClosed(EventArgs e)
		{
			if (!ssoReported)
				authMgr.ReportSsoResultsToMgr(false, String.Empty, String.Empty);

			base.OnClosed(e);
		}
		#endregion
	}
}
