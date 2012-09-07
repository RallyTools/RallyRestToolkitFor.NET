using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Rally.RestApi.Test
{
    [TestClass()]
    public class RequestTest
    {

        /// <summary>
        ///A test for Request Constructor
        ///</summary>
        [TestMethod()]
        public void RequestConstructor()
        {
            const string artifactName = "Defect";
            const string key = "Key";
            var fetch = new List<string>() { "Name" };
            const int limit = 500;
            const int pageSize = 1;
            const string order = "Name";
            const string project = "/Project/1243";
            const bool projectScopeDown = true;
            const bool projectScopeUp = true;
            const string workSpace = "workspace/1243";
            const int start = 200;
            var query = new Query("Field", Query.Operator.Equals, "Value");
            var target = new AdhocRequest(artifactName, key)
                                 {
                                     Fetch = fetch,
                                     Limit = limit,
                                     PageSize = pageSize,
                                     Order = order,
                                     Project = project,
                                     ProjectScopeDown = projectScopeDown,
                                     ProjectScopeUp = projectScopeUp,
                                     Query = query,
                                     Start = start,
                                     Workspace = workSpace

                                 };
            Assert.AreEqual(key, target.Key);
            Assert.AreEqual(artifactName, target.ArtifactName);
            Assert.AreEqual(fetch[0], target.Fetch[0]);
            Assert.AreEqual(fetch.Count, target.Fetch.Count);
            Assert.AreEqual(limit, target.Limit);
            Assert.AreEqual(pageSize, target.PageSize);
            Assert.AreEqual(start, target.Start);
            Assert.AreEqual(limit, target.Limit);
            Assert.AreEqual(order, target.Order);
            Assert.AreEqual(query, target.Query);
            Assert.AreEqual(project, target.Project);
            Assert.AreEqual(projectScopeDown, target.ProjectScopeDown);
            Assert.AreEqual(projectScopeUp, target.ProjectScopeUp);
            Assert.AreEqual(workSpace, target.Workspace);

        }

        /// <summary>
        ///A test for Request Constructor
        ///</summary>
        [TestMethod()]
        public void RequestConstructorDefault()
        {

            var query = new Query("Field", Query.Operator.Equals, "Value");
            var target = new AdhocRequest("type", "key");
            Assert.AreEqual(0, target.Fetch.Count);
            Assert.AreEqual(Request.MAX_PAGE_SIZE, target.Limit);
            Assert.AreEqual(Request.MAX_PAGE_SIZE, target.PageSize);
            Assert.AreEqual(1, target.Start);
            Assert.IsNull(target.Order);
            Assert.IsNull(target.Query);
            Assert.IsNull(target.Project);
            Assert.IsNull(target.ProjectScopeDown);
            Assert.IsNull(target.ProjectScopeUp);
            Assert.IsNull(target.Workspace);

        }

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
