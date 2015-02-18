using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Response;
using Rally.RestApi.Json;

namespace Rally.RestApi.Test
{
	/// <summary>
	/// Summary description for Example
	/// </summary>
	[TestClass]
	public class Example
	{
		public Example()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		/// <summary>
		/// This sample method is also provided in the Sandcastle documentation. If you make 
		/// changes here, please ensure they are propagated to there as well.
		/// </summary>
		[TestMethod]
		[Ignore]
		public void ExampleMethodText()
		{
			string username = "paul@acme.com";
			string password = "Just4Rally";
			// Initialize the REST API. You can specify a web service version if needed in the constructor.
			RallyRestApi restApi = new RallyRestApi();
			restApi.Authenticate(username, password, "https://preview.rallydev.com", proxy: null, allowSSO: false);

			//Create an item
			DynamicJsonObject toCreate = new DynamicJsonObject();
			toCreate["Name"] = "My Defect";
			CreateResult createResult = restApi.Create("defect", toCreate);

			//Update the item
			DynamicJsonObject toUpdate = new DynamicJsonObject();
			toUpdate["Description"] = "This is my defect.";
			OperationResult updateResult = restApi.Update(createResult.Reference,
											toUpdate);

			//Get the item
			DynamicJsonObject item = restApi.GetByReference(createResult.Reference);

			//Query for items
			Request request = new Request("defect");
			request.Fetch = new List<string>() { "Name", "Description", "FormattedID" };
			request.Query = new Query("Name", Query.Operator.Equals, "My Defect");
			QueryResult queryResult = restApi.Query(request);
			foreach (var result in queryResult.Results)
			{
				//Process item as needed
			}

			//Delete the item
			OperationResult deleteResult = restApi.Delete(createResult.Reference);
		}
	}
}
