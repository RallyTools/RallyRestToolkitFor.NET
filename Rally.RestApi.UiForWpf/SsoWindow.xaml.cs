using System.Reflection;
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
        private bool idpRedirectFlag;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public SsoWindow()
		{
			InitializeComponent();
			browser.LoadCompleted += browser_LoadCompleted;
			browser.Navigating += browser_Navigating;
		    idpRedirectFlag = true;
		    //browser.Navigated += (a, b) => HideScriptErrors(browser, true);
		}

		private void HideScriptErrors(WebBrowser browser, bool hide)
		{
			var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fiComWebBrowser == null) return;
			var objComWebBrowser = fiComWebBrowser.GetValue(browser);
			if (objComWebBrowser == null)
			{
				browser.Navigated += (o, s) => HideScriptErrors(browser, hide);
				return;
			}
			objComWebBrowser.GetType()
				.InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
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

		#region browser_Navigating
		/// <summary>
		/// WARNING **HACK** In reference to DE22791 these redirects are to avoid the JS errors
		/// thrown by the root rally login page, and redirect to the empty.sp endpoint which
		/// auths if the request already contains a valid ZSESSION id
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void browser_Navigating(object sender, NavigatingCancelEventArgs e)
		{
		    if (idpRedirectFlag)
		    {
		        string redirectUri = String.Empty;
                UriBuilder redirectUriBuilder = idpSsoRedirectBuilder(e.Uri);
		        while (redirectUriBuilder != null)
		        {
                    redirectUri = redirectUriBuilder.Uri.AbsoluteUri;
                    browser.Navigate(redirectUri);
		        }
            }
		}
		#endregion

        /// <summary>
        /// When attempting to navigate to the base rally url redirect to the IDP_SSO_ENDPOINT instead
        /// Also including test2cluster for testing
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        #region idpSsoRedirectBuilder
        private UriBuilder idpSsoRedirectBuilder(Uri uri)
        {
            string[] parseUri = uri.ToString().Split(new Char[] {'?', '&'});
            string targetResource = string.Empty;
            foreach (var param in parseUri)
            {
                if (param.Contains("TargetResource"))
                    targetResource = param;
            }
            //Dictionary<String, String> uriDict = parseUri.ToDictionary();

            if (uri.ToString().Equals(RallyRestApi.DEFAULT_SERVER)
                    || uri.ToString().Equals(RallyRestApi.DEFAULT_TEST2_SERVER)
                    || uri.ToString().Equals(""))
            {
                Uri redirectUrl = null;
                Uri.TryCreate(uri.AbsolutePath, UriKind.Absolute, out redirectUrl);

                var uriBuilder = new UriBuilder(uri + RallyRestApi.IDP_SSO_ENDPOINT);

                return uriBuilder;
            }

            return null;
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
