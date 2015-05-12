using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Json;

namespace Rally.RestApi.Test
{
    [TestClass]
    public class DynamicJsonObjectTest
    {
        /// <summary>
        ///A test for ToDictionary
        ///</summary>
        [TestMethod]
        public void DynamicJsonObject_ToDictionary()
        {
            dynamic djo = new DynamicJsonObject();
            djo.int1 = -19;
            djo.decimal1 = 1.21M;
            djo.string1 = "hi";

            var dict = djo.ToDictionary();

            Assert.IsNotNull(dict);
            Assert.AreEqual(-19, dict["int1"]);
            Assert.AreEqual(1.21M, dict["decimal1"]);
            Assert.AreEqual("hi", dict["string1"]);

            //Make sure the dictionary returned is readonly.
            dict["int1"] = -20; //change the value we got before.
            Assert.AreEqual(-19, djo.ToDictionary()["int1"]);  //reconvert and test again.
        }
    }
}