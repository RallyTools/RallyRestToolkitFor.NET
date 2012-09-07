using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rally.RestApi.Test
{
    [TestClass]
    public class HttpServiceTest
    {

        internal static HttpService GetService()
        {
            return new HttpService(IntegrationTestInfo.USER_NAME, IntegrationTestInfo.PASSWORD, IntegrationTestInfo.SERVER);
        }

        [TestMethod]
        public void Post()
        {
            var service = GetService();
            //2121901027.js
            var result = service.Post(new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/1.21/adhoc.js"),
                "{\"Key\": \"/HierarchicalRequirement?pagesize=1&query=(ObjectID = 3231965244)\"}");
            const string expected = "3231965244.js";
            Assert.IsTrue(result.Contains(expected));
        }

        [TestMethod]
        public void Get()
        {
            var service = GetService();
            //2121901027.js
            var result = service.Get(new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/1.21/defect/2121901027.js"));
            const string expected = "2010-09-15T17:05:55.709Z";
            Assert.IsTrue(result.Contains(expected));
        }

        [TestMethod]
        public void Delete()
        {
            var service = GetService();
            var result = service.Delete(new Uri(IntegrationTestInfo.SERVER + "/slm/webservice/1.21/defect/21218901027.js"));
            Assert.IsTrue(result.Contains("Object to delete cannot be null"));
        }

    }
}
