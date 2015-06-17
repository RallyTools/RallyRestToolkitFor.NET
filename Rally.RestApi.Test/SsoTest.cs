using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
            //WebRequest request = WebRequest.Create(test2cluster);
            //HttpWebRequest httpRequest = request as HttpWebRequest;
            //httpRequest.CookieContainer = new CookieContainer();
            //httpRequest.CookieContainer.Add(new Uri(test2cluster), new Cookie("ZSESSIONID", GetValidZSessionId()));
		}

		[TestMethod]
		public void foo()
		{
            //RallyRestApi api = new RallyRestApi();
            //RallyRestApi.AuthenticationResult result = api.Authenticate("jstupplebeen@rallydev.com", "Pr0m3th3u502", "rally1.rallydev.com");
            //SsoWebClient webClient = new SsoWebClient();

			//RallyRestApi restApi = new RallyRestApi(new ApiConsoleAuthManager());
			//restApi.Authenticate("ddl@2.com", "alegra99", test2cluster, null, true);
			//var foo = restApi.AuthenticationState;

            //var zsessionid = GetValidZSessionId();

            try
            {
                //using (var client = new WebClient())
                //{
                //    var response = client.UploadData("test2cluster.zuul1.f4tech.com:3000/key.js", "PUT",
                //        Encoding.UTF8.GetBytes("{\"username\":\"vcs_user@4278.com\", \"password\":\"RallyDev\"}"));
                //}

                /** ATTEMPT 2 **/
                //string payload = "{\"username\":\"vcs_user@4278.com\", \"password\":\"RallyDev\"}";
                //string pay2 = "fuck you";

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://test2cluster.zuul1.f4tech.com:3000/key.js");
                //request.Method = "PUT";
                //request.ContentType = "application/json";

                //ASCIIEncoding encoding = new ASCIIEncoding();
                //byte[] data = encoding.GetBytes(pay2);
                //request.ContentLength = data.Length;

                //request.ContentLength = payload.Length;
                //Stream dataStream = request.GetRequestStream();
                //var wat = new DataContractSerializer(payload.GetType());
                //wat.WriteObject(dataStream, payload);

                /** ATTEMPT 3 **/
                //using (var client = new WebClient())
                //{
                //    var values = new NameValueCollection();
                //    values["username"] = "vcs_user@4278.com";
                //    values["password"] = "RallyDev";
                //    //var url = new Uri("https://test2cluster.zuul1.f4tech.com:3000");
                //    var url = new Uri("http://www.google.com:80");
                //    //byte[] result = client.UploadValues(url, values);

                //    var result = client.DownloadString(url);
                //}

                //WebRequest request = WebRequest.Create("https://test2cluster.zuul1.f4tech.com:3000");
                //request.Credentials = CredentialCache.DefaultNetworkCredentials;
                //WebResponse response = request.GetResponse();
                //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                //Stream dataStream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                //string responseFromServer = reader.ReadToEnd();
                //Console.WriteLine(responseFromServer);
                //reader.Close();
                //response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
		}

		private void GetValidZSessionId()
		{
        }

		[ClassCleanup]
		public static void TearDown()
		{
		}
	}
}
