using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net;

namespace Rally.RestApi.Test
{
	[TestClass]
	public class SSOHelperTest
	{
		[ClassInitialize()]
		public static void Initialize(TestContext testContext)
		{
		}

		// TODO: Remove if it works
		//[TestMethod]
		//[DeploymentItem("Rally.RestApi.Test\\data\\")]
		//public void ParseSSOTokenPage()
		//{
		//  Cookie cookie = SSOHelper.ParseSSOLandingPage(getDataFromFile("SSOTokenPage.html"));
		//  Assert.IsNotNull(cookie);
		//  Assert.AreEqual(cookie.Name, "ZSESSIONID");
		//  Assert.AreEqual(cookie.Value, "khkjhkhkhkhkjhh");
		//  Assert.AreEqual(cookie.Domain, "us1.rallydev.com");
		//  Assert.AreEqual(cookie.Path, "/");
		//}

		private String getDataFromFile(String filename)
		{
			using (StreamReader sr = new StreamReader(filename))
			{
				return sr.ReadToEnd();
			}
		}
	}
}
