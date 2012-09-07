using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally.RestApi.Response;

namespace Rally.RestApi.Test
{
    /// <summary>
    ///This is a test class for OperationResultTest and is intended
    ///to contain all OperationResultTest Unit Tests
    ///</summary>
    [TestClass]
    public class OperationResultTest
    {        

        /// <summary>
        ///A test for Success
        ///</summary>
        [TestMethod]
        public void SuccessTest()
        {
            var errors = new List<string> {"Error"};
            var target = new OperationResult()
                             {
                                 Errors= errors
                             };
            Assert.IsFalse(target.Success);
            errors.Clear();
            Assert.IsTrue(target.Success);
        }        
    }
}