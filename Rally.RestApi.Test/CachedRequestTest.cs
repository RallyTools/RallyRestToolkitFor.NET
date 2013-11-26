using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Response;
using System;
using System.Collections.Generic;
using System.Net;

namespace Rally.RestApi.Test
{
	[TestClass()]
	public class CachedRequestTest
	{
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void QuerySchemaEndpointBadWsapiVersionTest()
		{
			RallyRestApi restApi = RallyRestApiTest.GetRallyRestApi(wsapiVersion: "1.43");
			restApi.GetTypes("");
		}

		[TestMethod]
		public void QuerySchemaEndpointForWorkspaceTest()
		{
			RallyRestApi restApi = RallyRestApiTest.GetRallyRestApi(wsapiVersion: "v2.0");
			long oid = 191977961;

			CacheableQueryResult results = restApi.GetTypes(String.Format("/workspace/{0}", oid));
			Assert.IsNotNull(results);
			Assert.IsFalse(results.IsCachedResult);

			results = restApi.GetTypes(String.Format("/workspace/{0}", oid));
			Assert.IsNotNull(results);
			Assert.IsTrue(results.IsCachedResult);
		}

		[TestMethod]
		public void QuerySchemaEndpointForSchemaTest()
		{
			RallyRestApi restApi = RallyRestApiTest.GetRallyRestApi(wsapiVersion: "v2.0");
			long oid = 1155237251;

			CacheableQueryResult results = restApi.GetTypes(String.Format("/schema/{0}", oid));
			Assert.IsNotNull(results);
			Assert.IsFalse(results.IsCachedResult);

			results = restApi.GetTypes(String.Format("/schema/{0}", oid));
			Assert.IsNotNull(results);
			Assert.IsTrue(results.IsCachedResult);
		}
	}
}
