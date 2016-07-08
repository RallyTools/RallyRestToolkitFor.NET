using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Auth;
using Rally.RestApi.Response;
using Rally.RestApi.Test.Properties;
using Rally.RestApi.Connection;
using Rally.RestApi.Json;
using System.Collections.Specialized;

namespace Rally.RestApi.Test
{
	[TestClass]
	public class RallyRestApiTest
	{
		public static RallyRestApi GetRallyRestApi(string userName = "", string password = "",
			string server = "", string wsapiVersion = "")
		{
			if (String.IsNullOrWhiteSpace(userName))
			{
				userName = Settings.Default.UserName;
			}

			if (String.IsNullOrWhiteSpace(password))
			{
				password = Settings.Default.Password;
			}

			if (String.IsNullOrWhiteSpace(server))
			{
				server = Settings.Default.TestServer;
			}

			if (String.IsNullOrWhiteSpace(wsapiVersion))
			{
				wsapiVersion = RallyRestApi.DEFAULT_WSAPI_VERSION;
			}

			RallyRestApi api = new RallyRestApi(webServiceVersion: wsapiVersion);
			api.Authenticate(userName, password, server);
			return api;
		}

		public static RallyRestApi GetRallyRestApiWithApiKey(string apiKey = "",
			string server = RallyRestApi.DEFAULT_SERVER, string wsapiVersion = "")
		{
			if (String.IsNullOrWhiteSpace(apiKey))
			{
				apiKey = Settings.Default.ApiKey;
			}

			RallyRestApi api = new RallyRestApi(webServiceVersion: wsapiVersion);
			RallyRestApi.AuthenticationResult authResult = api.AuthenticateWithApiKey(apiKey, server);
			Assert.AreEqual(RallyRestApi.AuthenticationResult.Authenticated, authResult);
			return api;
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
			
			// Now delete it
			TestHelperDeleteItem(restApi, response.Reference);
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
		public void AddToCollection2x()
		{
			RallyRestApi restApi = GetRallyRestApi2x();
			var itemRef = TestHelperCreateDefect(restApi);
			DynamicJsonObject newTask = new DynamicJsonObject();
			newTask["Name"] = "New Task Added via collection";
			NameValueCollection parameters = new NameValueCollection();
			parameters.Add("fetch", "FormattedID");
			OperationResult result = restApi.AddToCollection(itemRef, "Tasks", new List<DynamicJsonObject>() { newTask }, parameters);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(1, result.Results.Count);
			Assert.IsNotNull(result.Results[0]["FormattedID"]);

			// Now delete it
			TestHelperDeleteItem(restApi, itemRef);
		}

		[TestMethod]
		public void RemoveFromCollection2x()
		{
			RallyRestApi restApi = GetRallyRestApi2x();
			DynamicJsonObject newStory = new DynamicJsonObject();
			newStory["Name"] = "Test Story";
			var itemRef = restApi.Create("hierarchicalrequirement", newStory).Reference;
			DynamicJsonObject newDefect = new DynamicJsonObject();
			newDefect["Name"] = "New Defect Added via collection";
			newDefect["Requirement"] = itemRef;
			CreateResult newTaskResult = restApi.Create("defect", newDefect);

			DynamicJsonObject story = restApi.GetByReference(itemRef, "Defects");
			Assert.AreEqual(1, story["Defects"]["Count"]);

			DynamicJsonObject taskToRemove = new DynamicJsonObject();
			taskToRemove["_ref"] = newTaskResult.Reference;
			OperationResult result = restApi.RemoveFromCollection(itemRef, "Defects", new List<DynamicJsonObject>() { taskToRemove }, new NameValueCollection());

			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.Results.Count);
			story = restApi.GetByReference(itemRef, "Defects");
			Assert.AreEqual(0, story["Defects"]["Count"]);

			// Now delete the defect and story
			TestHelperDeleteItem(restApi, newTaskResult.Reference);
			TestHelperDeleteItem(restApi, itemRef);
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

		private void AssertCanDelete(RallyRestApi restApi, bool includeFullData = false)
		{
			// Create test defect
			var defect = TestHelperCreateDefect(restApi, includeFullData);
			var defectOid = Ref.GetOidFromRef(defect);

			OperationResult deleteResponse = restApi.Delete(Ref.GetRelativeRef(defect));
			dynamic testDefectEmpty = restApi.GetByReference(defect);
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
			// Create test defect
			var defect = TestHelperCreateDefect(restApi);
			var defectOid = Ref.GetOidFromRef(defect);

			var dynamicJson = new DynamicJsonObject();
			dynamicJson["Name"] = "Dont delete me please " + DateTime.Now.Second;
			OperationResult response = restApi.Update("Defect", defectOid, dynamicJson);
			Assert.AreEqual(0, response.Errors.Count);
			dynamic updateDefect = restApi.GetByReference("/Defect/" + defectOid + ".js");
			Assert.AreEqual(dynamicJson["Name"], updateDefect.Name);

			// Now delete it
			TestHelperDeleteItem(restApi, defect);
		}

		private string TestHelperCreateDefect(RallyRestApi restApi, bool includeFullData = false)
		{
			var dynamicJson = new DynamicJsonObject();
			dynamicJson["Name"] = "C# Json Rest Toolkit Test Defect";
			if (includeFullData)
			{
				dynamicJson["Owner"] = restApi.GetCurrentUser()["_ref"];
				dynamicJson["Package"] = "Package A";
			}

			CreateResult response = restApi.Create("defect", dynamicJson);
			Assert.AreEqual(0, response.Errors.Count);
			Assert.IsTrue(response.Reference.ToLower().Contains("defect"));

			return response.Reference;
		}

		private void TestHelperDeleteItem(RallyRestApi restApi, string reference)
		{
			OperationResult deleteResponse = restApi.Delete(Ref.GetRelativeRef(reference));
			dynamic testEmpty = restApi.GetByReference(reference);
			Assert.IsNull(testEmpty);
		}

		[TestMethod]
		public void GetByReferenceTest()
		{
			RallyRestApi restApi = GetRallyRestApi();

			// Create test defect
			var defect = TestHelperCreateDefect(restApi);
			var defectOid = Ref.GetOidFromRef(defect);

			dynamic response = restApi.GetByReference("/Defect/" + defectOid + ".js");
			Assert.AreEqual(defectOid, response.ObjectID.ToString());

			// Now delete it
			TestHelperDeleteItem(restApi, defect);
		}

		[TestMethod]
		public void GetByReferencePortfolioItemTest()
		{
			RallyRestApi restApi = GetRallyRestApi();
			Request fRequest = new Request("PortfolioItem/Feature");
			QueryResult queryResults = restApi.Query(fRequest);
			String featureRef = queryResults.Results.First()._ref;
			DynamicJsonObject feature = restApi.GetByReference(featureRef, "Name");
			Assert.IsNotNull(feature);
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
			VerifyAttributes(result125, false);
		}

		[TestMethod]
		public void TestAttribute2x()
		{
			RallyRestApi restApiv2 = GetRallyRestApi2x();
			QueryResult resultv2 = restApiv2.GetAttributesByType("Preference");
			VerifyAttributes(resultv2, true);
		}

		[TestMethod]
		public void FormatCreateString()
		{
			RallyRestApi restApi = GetRallyRestApi();
			NameValueCollection parameters = new NameValueCollection();
			parameters["fetch"] = "Name";
			Uri result = restApi.FormatCreateUri("defect", parameters);
			var expected = new Uri(Settings.Default.TestServer + "/slm/webservice/" + RallyRestApi.DEFAULT_WSAPI_VERSION + "/defect/create.js?fetch=Name");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void FormatUpdateString()
		{
			RallyRestApi restApi = GetRallyRestApi();
			NameValueCollection parameters = new NameValueCollection();
			parameters["fetch"] = "Name";
			Uri result = restApi.FormatUpdateUri("defect", "2121901027", parameters);
			var expected = new Uri(Settings.Default.TestServer + "/slm/webservice/" + RallyRestApi.DEFAULT_WSAPI_VERSION + "/defect/2121901027.js?fetch=Name");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void GetCurrentUser()
		{
			RallyRestApi restApi = GetRallyRestApi();
			dynamic user = restApi.GetCurrentUser();
			Assert.AreEqual("user", Ref.GetTypeFromRef(user._ref), "Type test");
			Assert.AreEqual(Settings.Default.UserName, user.UserName, "Name test");
		}

		[TestMethod]
		public void ApiKeyGetCurrentUser()
		{
			RallyRestApi restApi = GetRallyRestApiWithApiKey();
			dynamic user = restApi.GetCurrentUser();
			Assert.IsNotNull(user);
			Assert.AreEqual("user", Ref.GetTypeFromRef(user._ref), "Type test");
		}

		[TestMethod]
		public void ApiKeyCanDelete()
		{
			RallyRestApi restApi = GetRallyRestApiWithApiKey();
			AssertCanDelete(restApi, true);
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
			if (qr.TotalResultCount > request.Limit)
				Assert.AreEqual(request.Limit, qr.Results.Count());
			else
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

		[TestMethod]
		public void TestIdpLoginEndpointRedirect()
		{
			LoginDetails login = new LoginDetails(new ApiConsoleAuthManager());
			string redirectUrl = login.RedirectIfIdpPointsAtLoginSso("your-idp-url&TargetResource=https://rally1.rallydev.com/login/sso");
			Assert.AreEqual(redirectUrl, "your-idp-url&TargetResource=https://rally1.rallydev.com/slm/empty.sp");
		}

		private static void VerifyAttributes(QueryResult result, bool forWsapi2)
		{
			var list = (IEnumerable<object>)result.Results;
			IEnumerable<string> names = from DynamicJsonObject i in list.Cast<DynamicJsonObject>()
																	select i["Name"] as string;
			string[] expectedNames;
			if (forWsapi2)
				expectedNames = new string[] { "App Id", "Creation Date", "VersionId", "Object ID", "Name", "Project", "User", "Value", "Workspace", "ObjectUUID", "Type" };
			else
				expectedNames = new string[] { "App Id", "Creation Date", "Object ID", "Name", "Project", "User", "Value", "Workspace" };

			Assert.AreEqual(result.TotalResultCount, list.Count());
			Assert.AreEqual(expectedNames.Length, list.Count());
			IEnumerable<string> complement = expectedNames.Except(names);
			Assert.AreEqual(0, complement.Count());
		}
	}
}