using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Json;

namespace Rally.RestApi.Test
{
    [TestClass]
    public class DynamicJsonObjectTest
    {
        [TestMethod]
        public void DynamicJsonObject_ExistingField_Valid()
        {
            dynamic djo = new DynamicJsonObject();
            djo.someValue = "something";
            Assert.AreEqual("something", djo.someValue);
        }

        /// <summary>
        /// Null value in defined field on DynamicJsonObject should not throw exception
        /// </summary>
        [TestMethod]
        public void DynamicJsonObject_ExistingField_ShouldBeNull()
        {
            dynamic djo = new DynamicJsonObject();
            djo.nullValue = null;
            Assert.IsNull(djo.nullValue);
        }

        /// <summary>
        ///  Accessing non-existent field on DynamicJsonObject should NOT fail, and should return null
        /// </summary>
        [TestMethod]
        public void DynamicJsonObject_NonExistentValue_ShouldBeNull()
        {
            dynamic djo = new DynamicJsonObject();
            var thisCodeShouldNotThrowException = djo.nonexistent;
            Assert.IsNull(thisCodeShouldNotThrowException);
        }
    }
}
