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

        [TestMethod]
        [DeploymentItem("Rally.RestApi.Test\\data\\")]
        public void ParseSamlForm()
        {
            var ssoHelper = new SSOHelper();
            SSOHelper.FormInfo formInfo = ssoHelper.getFirstFormInfo(getDataFromFile("HtmlWithOneSamlForm.txt"));

            Assert.IsTrue(formInfo.isSamlForm());
            Assert.IsFalse(formInfo.isPasswordForm());
            Assert.AreEqual(formInfo.actionUrl, "https://some.host.com/actionUrl/endpoint1.html");

            List<SSOHelper.PostParam> postParams = formInfo.getSamlPostParams();
            Assert.IsTrue(postParams.Count == 2, String.Format("Params found not equal to 2.  Actually found {0}.",postParams.Count));
            Assert.IsNotNull(postParams.SingleOrDefault(x => x.name == "SAMLRequest" && x.value == "SamlRequestValue1"));
            Assert.IsNotNull(postParams.SingleOrDefault(x => x.name == "RelayState" && x.value == "RelayStateValue1"));
        }

        [TestMethod]
        [DeploymentItem("Rally.RestApi.Test\\data\\")]
        public void ParsePasswordForm()
        {
            var ssoHelper = new SSOHelper();
            SSOHelper.FormInfo formInfo = ssoHelper.getFirstFormInfo(getDataFromFile("HtmlWithOnePasswordForm.txt"));

            Assert.IsTrue(formInfo.isPasswordForm());
            Assert.IsFalse(formInfo.isSamlForm());
            Assert.AreEqual(formInfo.actionUrl, "/idp/resumeSAML20/idp/SSO.ping");

            Uri baseUri = new Uri("https://some.host.com/path");
            Assert.AreEqual(formInfo.getAbsoluteUri(baseUri), "https://some.host.com/idp/resumeSAML20/idp/SSO.ping");

            List<SSOHelper.PostParam> postParams = formInfo.getPasswordPostParams("SomeUsername","SomePassword");
            Assert.IsTrue(postParams.Count == 2, String.Format("Params found not equal to 2.  Actually found {0}.", postParams.Count));
            Assert.IsNotNull(postParams.SingleOrDefault(x => x.name == "pf.username" && x.value == "SomeUsername"));
            Assert.IsNotNull(postParams.SingleOrDefault(x => x.name == "pf.pass" && x.value == "SomePassword"));
        }

        [TestMethod]
        [DeploymentItem("Rally.RestApi.Test\\data\\")]
        public void ParseYahooPasswordForm()
        {
            var ssoHelper = new SSOHelper();
            SSOHelper.FormInfo formInfo = ssoHelper.getFirstFormInfo(getDataFromFile("YahooSSOPasswordForm.txt"));

            Assert.IsFalse(formInfo.isSamlForm());
            Assert.IsTrue(formInfo.isPasswordForm());
            Assert.AreEqual(formInfo.actionUrl, "/login/");

            List<SSOHelper.PostParam> postParams = formInfo.getPasswordPostParams("user","pass");
            Assert.IsTrue(postParams.Count == 2, String.Format("Params found not equal to 2.  Actually found {0}.", postParams.Count));
            Assert.IsNotNull(postParams.SingleOrDefault(x => x.name == "id" && x.value == "user"));
            Assert.IsNotNull(postParams.SingleOrDefault(x => x.name == "pass_word" && x.value == "pass"));
        }

        [TestMethod]
        [DeploymentItem("Rally.RestApi.Test\\data\\")]
        public void ParseSSOTokenPage()
        {
            Cookie cookie = SSOHelper.parseAuthCookie(getDataFromFile("SSOTokenPage.html"));
            Assert.IsNotNull(cookie);
            Assert.AreEqual(cookie.Name, "ZSESSIONID");
            Assert.AreEqual(cookie.Value, "khkjhkhkhkhkjhh");
            Assert.AreEqual(cookie.Domain, "us1.rallydev.com");
            Assert.AreEqual(cookie.Path, "/");
            Assert.IsTrue(cookie.Secure);
        }

        private String getDataFromFile(String filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
