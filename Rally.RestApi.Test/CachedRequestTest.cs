using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Response;
using Rally.RestApi.Web;
using System;
using System.Collections.Generic;
using System.Net;
using Rally.RestApi.Test.Properties;

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
			CookieAwareCacheableWebClient.ClearOldCacheFilesFromDisk(true);
			long oid = Settings.Default.WorkspaceOID;

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
			CookieAwareCacheableWebClient.ClearOldCacheFilesFromDisk(true);
			long oid = Settings.Default.ProjectOID;

			CacheableQueryResult results = restApi.GetTypes(String.Format("/schema/{0}", oid));
			Assert.IsNotNull(results);
			Assert.IsFalse(results.IsCachedResult);

			results = restApi.GetTypes(String.Format("/schema/{0}", oid));
			Assert.IsNotNull(results);
			Assert.IsTrue(results.IsCachedResult);
		}
	}
}
