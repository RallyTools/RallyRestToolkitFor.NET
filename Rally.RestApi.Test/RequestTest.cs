using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web;
using System;
using Rally.RestApi.Json;

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
			Assert.AreEqual(request.Endpoint, request2.Endpoint);
		}

		[TestMethod]
		public void CloneCollection()
		{
			DynamicJsonObject collection = new DynamicJsonObject();
			collection["_ref"] = "/hierarchicalrequirement/12345/defect.js";
			var request = new Request(collection);
			request.Fetch = new List<string>() { "Name", "FormattedID" };

			var request2 = request.Clone();
			Assert.AreEqual(string.Join(",", request.Fetch),
					string.Join(",", request2.Fetch));

			foreach (var parameterKey in request.Parameters.Keys)
			{
				Assert.AreEqual(request.Parameters[parameterKey],
						request2.Parameters[parameterKey]);
			}
			Assert.AreEqual(request.Endpoint, request2.Endpoint);
		}

		[TestMethod]
		public void FetchVsShallowFetch()
		{
			var request = new Request("Defect");
			request.UseShallowFetch = true;
			request.Fetch = new List<string>() { "Name", "FormattedID", "Parent[Name]" };
			Assert.IsTrue(request.RequestUrl.Contains("shallowFetch"));
			request.UseShallowFetch = false;
			Assert.IsFalse(request.RequestUrl.Contains("shallowFetch"));
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

		[TestMethod]
		public void TestCreateFromRef()
		{
			TestCreateFromRefHelper("https://rally1.rallydev.com/slm/webservice/v2.0/defect.js?pagesize=1&fetch=true&order=Name+desc,ObjectID&start=1",
					"https://rally1.rallydev.com/slm/webservice/v2.0");
			TestCreateFromRefHelper("https://rally1.rallydev.com/slm/webservice/v2.0/hierarchicalrequirement/12345/defect.js?pagesize=172&fetch=Name&order=ObjectID&start=57",
					"https://rally1.rallydev.com/slm/webservice/v2.0");
			TestCreateFromRefHelper("https://rally1.rallydev.com/slm/webservice/v2.0/Project/3195568271/Editors",
					"https://rally1.rallydev.com/slm/webservice/v2.0");
		}

		private void TestCreateFromRefHelper(string urlToCheck, string removePriorToValidation = null)
		{
			Request request = Request.CreateFromUrl(urlToCheck);
			string finalUrlToCheck = urlToCheck;
			if (removePriorToValidation != null)
				finalUrlToCheck = urlToCheck.Replace(removePriorToValidation, String.Empty);

			if (finalUrlToCheck.Contains("?"))
				Assert.AreEqual(finalUrlToCheck, request.RequestUrl);
			else
				Assert.AreEqual(finalUrlToCheck, request.Endpoint);
		}


	    [TestMethod]
	    public void TestAddParameter()
	    {
	        var request = new Request();
	        var count = request.Parameters.Count;
	        var result = request.AddParameter("something", "somethingValue");
	        Assert.IsTrue(result);
	        Assert.AreEqual(count + 1, request.Parameters.Count);
            Assert.IsTrue(request.Parameters.ContainsKey("something"));
            Assert.AreEqual("somethingValue", request.Parameters["something"]);

            //doesn't overwrite
            result = request.AddParameter("something", "somethingElse");
            Assert.IsFalse(result);
            Assert.AreNotEqual("somethingElse", request.Parameters["something"]);
            Assert.AreEqual("somethingValue", request.Parameters["something"]);
	    }
	}
}
