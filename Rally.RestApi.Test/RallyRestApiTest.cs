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
        private static long defectOid;

        internal RallyRestApi GetRallyRestApi(string userName = IntegrationTestInfo.USER_NAME, string password = IntegrationTestInfo.PASSWORD,
            string server = IntegrationTestInfo.SERVER, string wsapiVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
        {
            return new RallyRestApi(userName, password, server, wsapiVersion);
        }

        [TestMethod]
        public void Adhoc()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var req = new AdhocRequest("defect", "d") { Order = "Name desc", Query = new Query("Name", Query.Operator.Contains, "\"Test\"") };
            QueryResult response = restApi.BatchQuery(new[] { req })["d"];
            Assert.IsTrue(0 < response.Results.Count());
            foreach (dynamic result in response.Results)
            {
                Assert.IsTrue(result.Name.ToLower().Contains("test"));
            }
        }

        [TestMethod]
        public void AdhocGetUser()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var req = new AdhocRequest("user", "d");
            QueryResult response = restApi.BatchQuery(new[] { req })["d"];
            Assert.AreEqual(1, response.Results.Count());
            Assert.AreEqual(1, response.TotalResultCount);
            Assert.AreEqual(IntegrationTestInfo.USER_NAME, response.Results.First().UserName);
        }

        [TestMethod]
        public void AdhocSadPath()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var req = new AdhocRequest("defect", "d") { Order = "Name desc", Query = new Query("Name", Query.Operator.Equals, "\\\"Defect Test 3") };
            // should fail due to not quoted string
            try
            {
                restApi.BatchQuery(new[] { req });
            }
            catch (Exception ex)
            {
                var errors = (ArrayList)ex.Data["Errors"];
                Assert.AreEqual(1, errors.Count);
                Assert.IsTrue(
                    ((string)errors[0]).Contains(
                        "Cannot parse input stream due to I/O error as JSON document: Parse error"));
                Assert.AreEqual("Adhoc Query failed, Rally WSAPI Errors and Warnings included in exception data.",
                                ex.Message);
            }
        }

        [TestMethod]
        public void AdhocMultiple()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var req = new AdhocRequest("defect", "d") { Order = "Name desc", Query = new Query("Name", Query.Operator.Equals, "Jim") };
            var req2 = new AdhocRequest("defect", "OneDefect") { Order = "Name desc", Limit = 1 };
            var reqs = new List<AdhocRequest> { req, req2 };
            Dictionary<string, QueryResult> result = restApi.BatchQuery(reqs);
            Assert.AreEqual(2, result.Count);
            QueryResult oneDefectResult = result["OneDefect"];
            Assert.AreEqual(1, oneDefectResult.Results.ToList().Count, "One defect should return one defect");
            Assert.IsTrue(1 < oneDefectResult.TotalResultCount, "More than one total result");
            Assert.AreEqual(0, oneDefectResult.Warnings.ToList().Count, "No Warnings");
            Assert.AreEqual(0, oneDefectResult.Errors.ToList().Count, "No Errors");
        }

        [TestMethod]
        public void AdhocMultiplePages()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var AdhocRequest = new AdhocRequest("defect", "OneDefect") { Order = "Name desc", Limit = 2, PageSize = 1 };
            var reqs = new List<AdhocRequest> { AdhocRequest };
            Dictionary<string, QueryResult> result = restApi.BatchQuery(reqs);
            Assert.AreEqual(2, result.Values.First().Results.Count());
            Assert.IsTrue(result.Values.First().Results.Count() < result.Values.First().TotalResultCount);
        }

        [TestMethod]
        public void AdhocMultipleLimitTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var req = new AdhocRequest("defect", "OneDefect") { Order = "Name desc", PageSize = 20, Limit = 1 };
            var reqs = new List<AdhocRequest> { req };
            Dictionary<string, QueryResult> result = restApi.BatchQuery(reqs);
            Assert.AreEqual(1, result.Count);
            QueryResult oneDefectResult = result["OneDefect"];
            Assert.AreEqual(1, oneDefectResult.Results.ToList().Count, "One defect should return one defect");
            Assert.IsTrue(1 < oneDefectResult.TotalResultCount, "More than one total result");
            Assert.AreEqual(0, oneDefectResult.Warnings.ToList().Count, "No Warnings");
            Assert.AreEqual(0, oneDefectResult.Errors.ToList().Count, "No Errors");
        }

        [TestMethod]
        public void AdhocUri()
        {
            RallyRestApi restApi = GetRallyRestApi();
            Assert.AreEqual(new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/" + RallyRestApi.DEFAULT_WSAPI_VERSION + "/adhoc.js"), restApi.AdhocUri);
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
            CreateResult creationResult = restApi.Create(null, "defect", defect);
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
        public void GetAllowedAttributeValuesTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            DynamicJsonObject response = restApi.GetAllowedAttributeValues("hierarchicalrequirement", "schedulestate");
            Assert.IsTrue(response.HasMember("Accepted"));
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

        [TestMethod]
        public void AdhocRequestUrl()
        {
            RallyRestApi restApi = GetRallyRestApi();

            var req = new AdhocRequest("defect", "d") { Order = "Name desc", Query = new Query("Name", Query.Operator.Equals, "Jim") };
            string result = req.RequestUrl;
            // ObjectID is added to the order clause by the as a workaround for a server-side WSAPI bug
            const string expected = "/defect?pagesize=200&order=Name desc,ObjectID&query=(Name = Jim)&start=1&fetch=true";
            Assert.AreEqual(expected, result);

            var req2 = new AdhocRequest("defect", "d") { Query = new Query("Name", Query.Operator.Equals, "Jim") };
            string result2 = req2.RequestUrl;
            // Order by ObjectID clause is added as a workaround for a server-side WSAPI bug
            const string expected2 = "/defect?pagesize=200&order=ObjectID&query=(Name = Jim)&start=1&fetch=true";
            Assert.AreEqual(expected2, result2);

            var req3 = new AdhocRequest("defect", "d") { Order = "ObjectID", Query = new Query("Name", Query.Operator.Equals, "Jim") };
            string result3 = req3.RequestUrl;
            // ObjectID is not added twice to the order clause b/c it already exists
            const string expected3 = "/defect?pagesize=200&order=ObjectID&query=(Name = Jim)&start=1&fetch=true";
            Assert.AreEqual(expected3, result3);
        }

        [TestMethod]
        public void AdhocRequestPlaceHolderUrl()
        {
            RallyRestApi restApi = GetRallyRestApi();
            var req = new PlaceholderRequest("key/Tasks", "d")
                          {
                              Order = "Name desc",
                              Query = new Query("Name", Query.Operator.Equals, "Jim"),
                              Fetch = new List<string> { "Name" }
                          };
            string result = req.RequestUrl;
            const string expected = "${key/Tasks?fetch=Name}";
            Assert.AreEqual(expected, result);
        }

        private static void VerifyAttributes(QueryResult result)
        {
            var list = (IEnumerable<object>)result.Results;
            IEnumerable<string> names = from DynamicJsonObject i in list.Cast<DynamicJsonObject>()
                                        select i["Name"] as string;
            var expectedNames = new[] { "Creation Date", "Object ID", "Name", "Project", "User", "Value", "Workspace" };
            Assert.AreEqual(result.TotalResultCount, list.Count());
            Assert.AreEqual(expectedNames.Length, list.Count());
            IEnumerable<string> complement = expectedNames.Except(names);
            Assert.AreEqual(0, complement.Count());  
        }

        [TestMethod]
        public void TestAttribute()
        {
            //Test old
            RallyRestApi restApi124 = GetRallyRestApi(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER, "1.24");
            QueryResult result124 = restApi124.GetAttributesByType("Preference");
            VerifyAttributes(result124);
            

            //Test new
            RallyRestApi restApi125 = GetRallyRestApi(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER, "1.25");
            QueryResult result125 = restApi125.GetAttributesByType("Preference");
            VerifyAttributes(result125);
        }

        [TestMethod]
        public void FormatCreateString()
        {
            RallyRestApi restApi = GetRallyRestApi();
            Uri result = restApi.FormatCreateUri(null,"defect");
            var expected = new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/" + RallyRestApi.DEFAULT_WSAPI_VERSION + "/defect/create.js");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FormatUpdateString()
        {
            RallyRestApi restApi = GetRallyRestApi();
            Uri result = restApi.FormatUpdateUri("defect", 2121901027);
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
            dynamic subscription = restApi.GetSubscription();
            Assert.AreEqual("subscription", Ref.GetTypeFromRef(subscription._ref), "Type test");
            Assert.AreEqual(146524649, subscription.ObjectID, "Subscription Id");
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