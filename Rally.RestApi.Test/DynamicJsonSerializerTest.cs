using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rally.RestApi.Test
{


    [TestClass]
    public class DynamicJsonSerializerTest
    {
        /// <summary>
        ///A test for Deserialize
        ///</summary>
        [TestMethod]
        public void DeserializeTest()
        {

            dynamic expected = new DynamicJsonObject();
            expected.int1 = -19;
            expected.decimal1 = 1.21M;
            expected.string1 = "hi";
            dynamic obj1 = new DynamicJsonObject();
            obj1.int1 = 19;
            obj1.string1 = "hi";
            obj1.decimal1 = -1.21M;
            obj1.array1 = new int[0];
            obj1.array2 = new[] { "arrayValue1", "arrayValue1" };
            expected.obj1 = obj1;
            var target = new DynamicJsonSerializer();
            const string json = "{\"int1\":-19,\"decimal1\":1.21,\"string1\":\"hi\",\"obj1\":{\"int1\":19,\"string1\":\"hi\",\"decimal1\":-1.21,\"array1\":[],\"array2\":[\"arrayValue1\", \"arrayValue1\"]}}";
            var actual = target.Deserialize(json);
            Assert.AreEqual<DynamicJsonObject>(expected, actual);
        }

        /// <summary>
        ///A test for Deserialize
        ///</summary>
        [TestMethod]
        public void DeserializeTestArray()
        {
            var expected = new DynamicJsonObject();
            (expected as dynamic).array = new object[] { "arrayValue1", 8 };
            var target = new DynamicJsonSerializer();
            const string json = "{\"array\":[\"arrayValue1\",8]}";
            var actual = target.Deserialize(json);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Deserialize
        ///</summary>
        [TestMethod]
        public void SerializeTestArray()
        {
            var value = new DynamicJsonObject();
            (value as dynamic).array = new object[] { "arrayValue1", 8 };
            var target = new DynamicJsonSerializer();
            const string expected = "{\"array\":[\"arrayValue1\",8]}";
            var actual = target.Serialize(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Deserialize
        ///</summary>
        [TestMethod]
        public void DeserializeTestArrayWithObjects()
        {
            var expected = new DynamicJsonObject();
            var inner = new DynamicJsonObject();
            (inner as dynamic).objVal = 90;
            (expected as dynamic).array = new object[] { "arrayValue1", inner };
            var target = new DynamicJsonSerializer();
            const string json = "{\"array\":[\"arrayValue1\",{\"objVal\":90}]}";
            var actual = target.Deserialize(json);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Deserialize
        ///</summary>
        [TestMethod]
        public void SerializeTestArrayWithObjects()
        {
            var value = new DynamicJsonObject();
            var inner = new DynamicJsonObject();
            (inner as dynamic).objVal = 90;
            (value as dynamic).array = new object[] { "arrayValue1", inner };
            var target = new DynamicJsonSerializer();
            const string expected = "{\"array\":[\"arrayValue1\",{\"objVal\":90}]}";
            var actual = target.Serialize(value);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for Deserialize
        ///</summary>
        [TestMethod]
        public void SerializeTestSimpleObject()
        {
            var value = new DynamicJsonObject();
            var target = new DynamicJsonSerializer();
            const string expected = "{}";
            var actual = target.Serialize(value);
            Assert.AreEqual(expected, actual);
        }

        //{"array":["arrayValue1",{"objVal":90}]}>. Actual:<
        //{"array":["arrayValue1",{"Dictionary":{"objVal":90}}]}

        /// <summary>
        ///A test for Serialize
        ///</summary>
        [TestMethod]
        public void SerializeTest()
        {
            var target = new DynamicJsonSerializer();
            dynamic value = new DynamicJsonObject();
            value.int1 = -19;
            value.decimal1 = 1.21M;
            value.string1 = "hi";
            dynamic obj1 = new DynamicJsonObject();
            obj1.int1 = 19;
            obj1.string1 = "hi";
            obj1.decimal1 = -1.21M;
            value.obj1 = obj1;
            const string expected = "{\"int1\":-19,\"decimal1\":1.21,\"string1\":\"hi\",\"obj1\":{\"int1\":19,\"string1\":\"hi\",\"decimal1\":-1.21}}";
            string actual = target.Serialize(value);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for Serialize
        ///</summary>
        [TestMethod]
        public void RoundTripTest()
        {
            dynamic val1 = new DynamicJsonObject();
            val1.int1 = -19;
            val1.decimal1 = 1.21M;
            val1.string1 = "hi";
            dynamic obj1 = new DynamicJsonObject();
            obj1.int1 = 19;
            obj1.string1 = "hi";
            obj1.decimal1 = -1.21M;
            val1.obj1 = obj1;
            var target = new DynamicJsonSerializer();
            string json = target.Serialize(val1);
            dynamic val2 = target.Deserialize(json);

            Assert.AreEqual(val2.int1, -19);
            Assert.AreEqual(val2.decimal1, 1.21M);
            Assert.AreEqual(val2.string1, "hi");
            Assert.AreEqual(val2.obj1.int1, obj1.int1);
            Assert.AreEqual(val2.obj1.string1, obj1.string1);
            Assert.AreEqual(val2.obj1.decimal1, obj1.decimal1);
        }


    }
}
