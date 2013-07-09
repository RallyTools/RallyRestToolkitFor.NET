using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Response;

namespace Rally.RestApi.Test
{
    [TestClass]
    public class RallyRestApiTest
    {
        private static string defectOid;

        internal RallyRestApi GetRallyRestApi(string userName = IntegrationTestInfo.USER_NAME, string password = IntegrationTestInfo.PASSWORD,
            string server = IntegrationTestInfo.SERVER, string wsapiVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
        {
            return new RallyRestApi(userName, password, server, wsapiVersion);
        }

        [TestMethod]
        public void CreateTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var dynamicJson = new DynamicJsonObject();
            dynamicJson["Name"] = "C# Json Rest Toolkit Test Defect";
            CreateResult response = restApi.Create(null, "defect", dynamicJson);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(0, response.Warnings.Count);
            Assert.IsTrue(response.Reference.ToLower().Contains("defect"));
            dynamic testDefect = restApi.GetByReference(response.Reference);
            Assert.AreEqual(dynamicJson["Name"], testDefect.Name);
            defectOid = Ref.GetOidFromRef(response.Reference);
        }

        [TestMethod]
        public void CreateSadPathTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var defect = new DynamicJsonObject();
            defect["Name"] = "Sample Defect with invalid field";
            defect["Iteration"] = "Foo";
            CreateResult creationResult = restApi.Create("defect", defect);
            Assert.IsNull(creationResult.Reference);
            Assert.AreEqual(1, creationResult.Errors.Count);
            Assert.IsFalse(creationResult.Success);
        }

        [TestMethod]
        public void DeleteTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var dynamicJson = new DynamicJsonObject();
            dynamicJson["Name"] = "C# Json Rest Toolkit Test Defect";
            CreateResult response = restApi.Create(null, "defect", dynamicJson);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(0, response.Warnings.Count);
            Assert.IsTrue(response.Reference.ToLower().Contains("defect"));
            OperationResult deleteResponse = restApi.Delete(null,Ref.GetRelativeRef(response.Reference));
            dynamic testDefectEmpty = restApi.GetByReference(response.Reference);
            Assert.IsNull(testDefectEmpty);
        }

        [TestMethod]
        public void UpdateTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var dynamicJson = new DynamicJsonObject();
            dynamicJson["Name"] = "Dont delete me please " + DateTime.Now.Second;
            OperationResult response = restApi.Update("Defect", defectOid, dynamicJson);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(0, response.Warnings.Count);
            dynamic updateDefect = restApi.GetByReference("/Defect/" + defectOid + ".js");
            Assert.AreEqual(dynamicJson["Name"], updateDefect.Name);
        }

        //        //
        [TestMethod]
        public void GetByReferenceTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            dynamic response = restApi.GetByReference("/Defect/" + defectOid + ".js");
            Assert.AreEqual(defectOid, response.ObjectID);
        }

        [TestMethod]
        public void GetAllowedAttributeValuesTest1x()
        {
            RallyRestApi restApi = GetRallyRestApi(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER, "1.43");
            QueryResult response = restApi.GetAllowedAttributeValues("hierarchicalrequirement", "schedulestate");
            Assert.IsNotNull(response.Results.SingleOrDefault(a => a.StringValue == "Accepted"));
        }

        [TestMethod]
        public void GetAllowedAttributeValuesTest2x()
        {
            RallyRestApi restApi = GetRallyRestApi(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER, "v2.0");
            QueryResult response = restApi.GetAllowedAttributeValues("hierarchicalrequirement", "schedulestate");
            Assert.IsNotNull(response.Results.SingleOrDefault(a => a.StringValue == "Accepted"));
        }

        [TestMethod]
        public void GetByReferenceUserTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            dynamic response = restApi.GetByReference("/user.js");
            Assert.IsNotNull(response.ObjectID);
        }

        [TestMethod]
        public void GetByReferenceSubscriptionTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            dynamic response = restApi.GetByReference("/subscription.js");
            Assert.IsNotNull(response.ObjectID);
        }

        private static void VerifyAttributes(QueryResult result)
        {
            var list = (IEnumerable<object>)result.Results;
            IEnumerable<string> names = from DynamicJsonObject i in list.Cast<DynamicJsonObject>()
                                        select i["Name"] as string;
            var expectedNames = new[] { "App Id", "Creation Date", "Object ID", "Name", "Project", "User", "Value", "Workspace" };
            Assert.AreEqual(result.TotalResultCount, list.Count());
            Assert.AreEqual(expectedNames.Length, list.Count());
            IEnumerable<string> complement = expectedNames.Except(names);
            Assert.AreEqual(0, complement.Count());  
        }

        [TestMethod]
        public void TestAttribute1x()
        {
            RallyRestApi restApi125 = GetRallyRestApi(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER, "1.43");
            QueryResult result125 = restApi125.GetAttributesByType("Preference");
            VerifyAttributes(result125);
        }

        [TestMethod]
        public void TestAttribute2x()
        {
            RallyRestApi restApiv2 = GetRallyRestApi(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER, "v2.0");
            QueryResult resultv2 = restApiv2.GetAttributesByType("Preference");
            VerifyAttributes(resultv2);
        }

        [TestMethod]
        public void FormatCreateString()
        {
            RallyRestApi restApi = GetRallyRestApi();
            Uri result = restApi.FormatCreateUri(null, "defect");
            var expected = new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/" + RallyRestApi.DEFAULT_WSAPI_VERSION + "/defect/create.js");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FormatUpdateString()
        {
            RallyRestApi restApi = GetRallyRestApi();
            Uri result = restApi.FormatUpdateUri("defect", "2121901027");
            var expected = new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/" + RallyRestApi.DEFAULT_WSAPI_VERSION + "/defect/2121901027.js");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetCurrentUser()
        {
            RallyRestApi restApi = GetRallyRestApi();
            dynamic user = restApi.GetCurrentUser();
            Assert.AreEqual("user", Ref.GetTypeFromRef(user._ref), "Type test");
            Assert.AreEqual(IntegrationTestInfo.USER_NAME, user.UserName, "Name test");
        }


        [TestMethod]
        public void GetSubscription()
        {
            RallyRestApi restApi = GetRallyRestApi();
            dynamic user = restApi.GetCurrentUser("Subscription", "ObjectID");
            dynamic subscription = restApi.GetSubscription();

            Assert.AreEqual("subscription", Ref.GetTypeFromRef(subscription._ref), "Type test");
            Assert.AreEqual(user.Subscription.ObjectID, subscription.ObjectID, "Subscription Id");
        }

        [TestMethod]
        public void GetOpenDefects()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var request = new Request("defect")
                              {
                                  Query = new Query("State", Query.Operator.Equals, "Open"),
                                  PageSize = 1,
                                  Order = "ObjectID"
                              };
            QueryResult qr = restApi.Query(request);
            Assert.IsTrue(qr.Success, "Query Success");
            Assert.AreEqual(qr.TotalResultCount, qr.Results.Count());
        }

        [TestMethod]
        public void QueryMultiplePages()
        {
            var restApi = GetRallyRestApi();
            var request = new Request("defect")
                                   {
                                       Order = "Name desc",
                                       Limit = 2,
                                       PageSize = 1
                                   };
            var result = restApi.Query(request);
            Assert.AreEqual(2, result.Results.Count());
            Assert.IsTrue(result.Results.Count() < result.TotalResultCount);
        }

        [TestMethod]
        public void QueryMultiplePagesSinglePageSize()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var request = new Request("defect")
            {
                Order = "ObjectID",
                Fetch = new List<string>() { "ObjectID" },
                Limit = 900,
                PageSize = 1
            };
            var result = restApi.Query(request);
            long previousId = 0;
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Results.Count() > 0);
            foreach (DynamicJsonObject r in result.Results)
            {
                long id = r["ObjectID"];
                Assert.IsTrue(previousId <= id, string.Format("{1} expected to be before {0}.", id, previousId));
                previousId = id;
            }
        }
    }
}