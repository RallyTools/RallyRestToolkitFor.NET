using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rally.RestApi.Test
{
    
 [TestClass()]
    public class RefTest
    {


        /// <summary>
        ///A test for GetRelativeRef
        ///</summary>
        [TestMethod()]
        public void GetRelativeRefTest()
        {
            const string absoluteRef = "https://rally1.rallydev.com/slm/webservice/1.23/hierarchicalrequirement/415737";
            const string expected = "/hierarchicalrequirement/415737"; 
            var actual = Ref.GetRelativeRef(absoluteRef);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetRelativeRef
        ///</summary>
        [TestMethod()]
        public void GetRelativeRefJsExtentionTest()
        {
            const string absoluteRef = "https://rally1.rallydev.com/slm/webservice/1.23/hierarchicalrequirement/415737.js";
            const string expected = "/hierarchicalrequirement/415737";
            var actual = Ref.GetRelativeRef(absoluteRef);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetRelativeRef
        ///</summary>
        [TestMethod()]
        public void GetRelativeRefWithRelativeRef()
        {            
            const string expected = "/hierarchicalrequirement/415737";
            var actual = Ref.GetRelativeRef(expected);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetTypeFromRef
        ///</summary>
        [TestMethod()]
        public void GetTypeFromRefTest()
        {
            const string reference = "/defect/12342.js";
            const string expected = "defect";
            string actual = Ref.GetTypeFromRef(reference);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetOidFromRef
        ///</summary>
        [TestMethod()]
        public void GetOidFromRefTest()
        {
            const string reference = "/defect/12342.js";
            long actual = Ref.GetOidFromRef(reference); 
            Assert.AreEqual(12342L, actual);
        }
    }
}
