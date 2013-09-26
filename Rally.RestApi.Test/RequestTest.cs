using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web;

namespace Rally.RestApi.Test
{
    [TestClass()]
    public class RequestTest
    {
        [TestMethod]
        public void Clone()
        {
            var request = new Request("Defect");
            request.Fetch = new List<string>() { "Name", "FormattedID" };

            var request2 = request.Clone();
            Assert.AreEqual(string.Join(",", request.Fetch),
                string.Join(",", request2.Fetch));

            foreach (var parameterKey in request.Parameters.Keys)
            {
                Assert.AreEqual(request.Parameters[parameterKey],
                    request2.Parameters[parameterKey]);
            }
        }

        [TestMethod]
        public void TestEndpointSubscription()
        {
            var request = new Request("Subscription");
            request.Fetch = new List<string>() { "Name" };

            Assert.AreEqual("/subscriptions", request.Endpoint);
        }

        [TestMethod]
        public void TestEndpointUser()
        {
            var request = new Request("User");
            request.Fetch = new List<string>() { "FirstName" };

            Assert.AreEqual("/users", request.Endpoint);
        }

        [TestMethod]
        public void TestEndpointDefect()
        {
            var request = new Request("Defect");
            request.Fetch = new List<string>() { "Name" };

            Assert.AreEqual("/defect", request.Endpoint);
        }

        [TestMethod]
        public void TestEndpointCollection()
        {
            DynamicJsonObject collection = new DynamicJsonObject();
            collection["_ref"] = "https://rally1.rallydev.com/slm/webservice/v2.0/defect/12345/tasks";

            var request = new Request(collection);

            Assert.AreEqual("/defect/12345/tasks", request.Endpoint);
        }

        [TestMethod]
        public void TestQueryStringUrlEncoded()
        {
            string query = "(Iteration.StartDate > Today+3)";
            var request = new Request("Defect");
            request.Fetch = new List<string>() { "Name" };
            request.Query = new Query("(Iteration.StartDate > Today+3)");
            request.RequestUrl.Contains(HttpUtility.UrlEncode(query));
        }
    }
}
