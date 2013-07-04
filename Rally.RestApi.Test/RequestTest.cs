using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
        public void TestEndpointNameSubscription()
        {
            var request = new Request("Subscription");
            request.Fetch = new List<string>() { "Name" };

            Assert.AreEqual("subscriptions", request.EndpointName);
        }

        [TestMethod]
        public void TestEndpointNameUser()
        {
            var request = new Request("User");
            request.Fetch = new List<string>() { "FirstName" };

            Assert.AreEqual("users", request.EndpointName);
        }

        [TestMethod]
        public void TestEndpointNameDefect()
        {
            var request = new Request("Defect");
            request.Fetch = new List<string>() { "Name" };

            Assert.AreEqual("defect", request.EndpointName);
        }
    }
}
