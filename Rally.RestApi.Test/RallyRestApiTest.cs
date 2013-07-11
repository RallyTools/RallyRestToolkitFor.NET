﻿using System;
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

        RallyRestApi GetRallyRestApi(string userName = IntegrationTestInfo.USER_NAME, string password = IntegrationTestInfo.PASSWORD,
            string server = IntegrationTestInfo.SERVER, string wsapiVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
        {
            return new RallyRestApi(userName, password, server, wsapiVersion);
        }

        RallyRestApi GetRallyRestApi1x()
        {
            return GetRallyRestApi(wsapiVersion: "1.43");
        }

        RallyRestApi GetRallyRestApi2x()
        {
            return GetRallyRestApi(wsapiVersion: "v2.0");
        }

        [TestMethod]
        public void BadAuth1x()
        {
            try
            {
                RallyRestApi restApi = GetRallyRestApi(userName: "foo", wsapiVersion: "1.43");
                restApi.GetSubscription();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "The remote server returned an error: (401) Unauthorized.");
            }
        }

        [TestMethod]
        public void BadAuth2x()
        {
            try
            {
                RallyRestApi restApi = GetRallyRestApi(userName: "foo", wsapiVersion: "v2.0");
                restApi.GetSubscription();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "The remote server returned an error: (401) Unauthorized.");
            }
        }

        [TestMethod]
        public void CreateTest1x()
        {
            RallyRestApi restApi = GetRallyRestApi1x();
            AssertCanCreate(restApi);
        }

        [TestMethod]
        public void CreateTest2x()
        {
            RallyRestApi restApi = GetRallyRestApi2x();
            AssertCanCreate(restApi);
        }

        private void AssertCanCreate(RallyRestApi restApi)
        {
            var dynamicJson = new DynamicJsonObject();
            dynamicJson["Name"] = "C# Json Rest Toolkit Test Defect";
            CreateResult response = restApi.Create("defect", dynamicJson);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.IsTrue(response.Reference.ToLower().Contains("defect"));
            dynamic testDefect = restApi.GetByReference(response.Reference);
            Assert.AreEqual(dynamicJson["Name"], testDefect.Name);
            defectOid = Ref.GetOidFromRef(response.Reference);
        }

        [TestMethod]
        public void CreateSadPath1x()
        {
            RallyRestApi restApi = GetRallyRestApi1x();
            AssertCreateFailure(restApi);
        }

        [TestMethod]
        public void CreateSadPath2x()
        {
            RallyRestApi restApi = GetRallyRestApi2x();
            AssertCreateFailure(restApi);
        }

        private void AssertCreateFailure(RallyRestApi restApi)
        {
            var defect = new DynamicJsonObject();
            defect["Name"] = "Sample Defect with invalid field";
            defect["Iteration"] = "Foo";
            CreateResult creationResult = restApi.Create("defect", defect);
            Assert.IsNull(creationResult.Reference);
            Assert.AreEqual(1, creationResult.Errors.Count);
            Assert.IsFalse(creationResult.Success);
        }

        [TestMethod]
        public void Delete1x()
        {
            RallyRestApi restApi = GetRallyRestApi1x();
            AssertCanDelete(restApi);
        }

        [TestMethod]
        public void Delete2x()
        {
            RallyRestApi restApi = GetRallyRestApi2x();
            AssertCanDelete(restApi);
        }

        private void AssertCanDelete(RallyRestApi restApi)
        {
            var dynamicJson = new DynamicJsonObject();
            dynamicJson["Name"] = "C# Json Rest Toolkit Test Defect";
            CreateResult response = restApi.Create("defect", dynamicJson);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.IsTrue(response.Reference.ToLower().Contains("defect"));
            OperationResult deleteResponse = restApi.Delete(Ref.GetRelativeRef(response.Reference));
            dynamic testDefectEmpty = restApi.GetByReference(response.Reference);
            Assert.IsNull(testDefectEmpty);
        }

        [TestMethod]
        public void Update1x()
        {
            RallyRestApi restApi = GetRallyRestApi1x();
            AssertCanUpdate(restApi);
        }

        [TestMethod]
        public void Update2x()
        {
            RallyRestApi restApi = GetRallyRestApi2x();
            AssertCanUpdate(restApi);
        }

        private void AssertCanUpdate(RallyRestApi restApi)
        {
            var dynamicJson = new DynamicJsonObject();
            dynamicJson["Name"] = "Dont delete me please " + DateTime.Now.Second;
            OperationResult response = restApi.Update("Defect", defectOid, dynamicJson);
            Assert.AreEqual(0, response.Errors.Count);
            dynamic updateDefect = restApi.GetByReference("/Defect/" + defectOid + ".js");
            Assert.AreEqual(dynamicJson["Name"], updateDefect.Name);
        }

        [TestMethod]
        public void GetByReferenceTest()
        {
            RallyRestApi restApi = GetRallyRestApi();
            dynamic response = restApi.GetByReference("/Defect/" + defectOid + ".js");
            Assert.AreEqual(defectOid, response.ObjectID.ToString());
        }

        [TestMethod]
        public void GetAllowedAttributeValuesTest1x()
        {
            RallyRestApi restApi = GetRallyRestApi1x();
            QueryResult response = restApi.GetAllowedAttributeValues("hierarchicalrequirement", "schedulestate");
            Assert.IsNotNull(response.Results.SingleOrDefault(a => a.StringValue == "Accepted"));
        }

        [TestMethod]
        public void GetAllowedAttributeValuesTest2x()
        {
            RallyRestApi restApi = GetRallyRestApi2x();
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

       

        [TestMethod]
        public void TestAttribute1x()
        {
            RallyRestApi restApi125 = GetRallyRestApi1x();
            QueryResult result125 = restApi125.GetAttributesByType("Preference");
            VerifyAttributes(result125);
        }

        [TestMethod]
        public void TestAttribute2x()
        {
            RallyRestApi restApiv2 = GetRallyRestApi2x();
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

        [TestMethod]
        public void TestIsWsapi2()
        {
            var restApi = GetRallyRestApi2x();
            Assert.IsTrue(restApi.IsWsapi2);
        }

        [TestMethod]
        public void TestIsNotWsapi2()
        {
            var restApi = GetRallyRestApi1x();
            Assert.IsFalse(restApi.IsWsapi2);
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
    }
}