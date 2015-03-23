using mshtml;
using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rally.RestApi.UiForWpf
{
	/// <summary>
	/// Interaction logic for SsoWindow.xaml
	/// </summary>
	internal partial class SsoWindow : Window
	{
		bool ssoReported = false;
		RestApiAuthMgrWpf authMgr;
		private Uri ssoUrl;
		private Uri idpUrl = null;
		private WebProxy idpProxy = null;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public SsoWindow()
		{
			InitializeComponent();
			browser.LoadCompleted += browser_LoadCompleted;
		}
		#endregion

		#region ShowSsoPage
		/// <summary>
		/// Shows the specified SSO URL to the user.
		/// </summary>
		internal void ShowSsoPage(RestApiAuthMgrWpf authMgr, Uri ssoUrl)
		{
			if (authMgr == null)
				throw new ArgumentNullException("authMgr", "You must provide an authorization manager.");

			if (ssoUrl == null)
				throw new ArgumentNullException("ssoUrl", "You must provide a URL for completing SSO authentication.");

			try
			{
				this.authMgr = authMgr;
				if (authMgr.Api.ConnectionInfo.IdpServer != null)
				{
					idpUrl = authMgr.Api.ConnectionInfo.IdpServer;
					idpProxy = authMgr.Api.ConnectionInfo.Proxy;
				}

				this.ssoUrl = ssoUrl;
				SetUrl();
				Show();
			}
			catch
			{
				Dispatcher.Invoke(SetUrl);
				Dispatcher.Invoke(Show);
				// If current thread is not the dispacher thread, then we can't launch the window.
				// Fail and consider SSO not available.
			}
		}
		#endregion

		private void SetUrl()
		{
			browser.Source = ssoUrl;
		}

		#region browser_LoadCompleted
		void browser_LoadCompleted(object sender, NavigationEventArgs e)
		{
			try
			{
				HTMLDocumentClass document = ((HTMLDocumentClass)browser.Document);
				if (document != null)
				{
					Uri documentUri = new Uri(document.url);
					CookieContainer cookiejar = GetUriCookieContainer(documentUri);
					if (cookiejar == null)
						return;

					CookieCollection cookieCollection = cookiejar.GetCookies(documentUri);
					if (cookieCollection == null)
						return;

					foreach (Cookie currentCookie in cookieCollection)
					{
						if (currentCookie.Name.Equals(RallyRestApi.ZSessionID, StringComparison.InvariantCultureIgnoreCase))
						{
							WindowState = WindowState.Minimized;

							string rallyServer = documentUri.GetLeftPart(UriPartial.Authority);
							if (idpUrl != null)
								authMgr.Api.CreateIdpAuthentication(idpUrl, idpProxy);

							authMgr.ReportSsoResultsToMgr(true, rallyServer, currentCookie.Value);
							ssoReported = true;
							Close();
						}
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
