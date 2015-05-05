using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Auth;
using Rally.RestApi.Connection;
using Rally.RestApi.Web;

namespace Rally.RestApi.Test
{
	[TestClass]
	public class SsoTest
	{
		private const string test2cluster = "https://test2cluster.rallydev.com";

		[ClassInitialize]
		public static void Setup(TestContext testContext)
		{
		}

		[TestMethod]
		public void IdpSsoTest()
		{
			WebRequest request = WebRequest.Create(test2cluster);
			HttpWebRequest httpRequest = request as HttpWebRequest;
			httpRequest.CookieContainer = new CookieContainer();
			httpRequest.CookieContainer.Add(new Uri(test2cluster), new Cookie("ZSESSIONID", GetValidZSessionId()));
		}

		[TestMethod]
		public void foo()
		{
			RallyRestApi api = new RallyRestApi();
			RallyRestApi.AuthenticationResult result = api.Authenticate("jstupplebeen@rallydev.com", "Pr0m3th3u502", "rally1.rallydev.com");



			SsoWebClient webClient = new SsoWebClient();

			//RallyRestApi restApi = new RallyRestApi(new ApiConsoleAuthManager());
			//restApi.Authenticate("ddl@2.com", "alegra99", test2cluster, null, true);
			//var foo = restApi.AuthenticationState;
		}

		private static string GetValidZSessionId()
		{
			return "";
		}

		[ClassCleanup]
		public static void TearDown()
		{
		}
	}
}
