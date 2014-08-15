using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rally.RestApi.Test
{
	[TestClass()]
	public class RefTest
	{
		[TestMethod()]
		public void ShouldDetectValidRefs()
		{
			Assert.IsTrue(Ref.IsRef("/defect/1234"), "Valid relative ref");
			Assert.IsTrue(Ref.IsRef("/defect/1234.js"), "Valid relative ref w/ extension");
			Assert.IsTrue(Ref.IsRef("/typedefinition/-1234.js"), "Valid built-in typedef relative ref w/ extension");
			Assert.IsTrue(Ref.IsRef("https://rally1.rallydev.com/slm/webservice/1.32/defect/1234"), "Valid absolute ref");
			Assert.IsTrue(Ref.IsRef("http://rally1.rallydev.com/slm/webservice/1.32/defect/1234.js"), "Valid absolute ref w/ extension");
			Assert.IsTrue(Ref.IsRef("https://trial.rallydev.com/slm/webservice/v2.0/typedefinition/-51004"), "Valid ref with negative oid");
		}

		[TestMethod()]
		public void ShouldDetectValidDynatypeRefs()
		{
			Assert.IsTrue(Ref.IsRef("/portfolioitem/feature/1234"), "Valid relative ref");
			Assert.IsTrue(Ref.IsRef("/portfolioitem/feature/1234.js"), "Valid relative ref w/ extension");
			Assert.IsTrue(Ref.IsRef("https://rally1.rallydev.com/slm/webservice/1.32/portfolioitem/feature/1234"), "Valid absolute ref");
			Assert.IsTrue(Ref.IsRef("http://rally1.rallydev.com/slm/webservice/1.32/portfolioitem/feature/1234.js"), "Valid absolute ref w/ extension");
		}

		[TestMethod()]
		public void ShouldDetectInvalidRefs()
		{
			Assert.IsFalse(Ref.IsRef("/defect"), "Invalid ref");
			Assert.IsFalse(Ref.IsRef("https://rally1.rallydev.com/slm/webservice/1.32/defect/abc.js"), "Invalid ref");
			Assert.IsFalse(Ref.IsRef(null), "A null ref");
			Assert.IsFalse(Ref.IsRef(""), "An empty string");
		}

		[TestMethod()]
		public void ShouldReturnValidRelativeRefs()
		{
			Assert.AreEqual(Ref.GetRelativeRef("/defect/1234"), "/defect/1234", "Already relative ref");
			Assert.AreEqual(Ref.GetRelativeRef("/defect/1234.js"), "/defect/1234", "Already relative ref");
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/1.32/defect/1234"), "/defect/1234", "Absolute ref");
		}

		[TestMethod()]
		public void ShouldReturnValidDynatypeRelativeRefs()
		{
			Assert.AreEqual(Ref.GetRelativeRef("/portfolioitem/feature/1234"), "/portfolioitem/feature/1234", "Already relative ref");
			Assert.AreEqual(Ref.GetRelativeRef("/portfolioitem/feature/1234.js"), "/portfolioitem/feature/1234", "Already relative ref");
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/1.32/portfolioitem/feature/1234"), "/portfolioitem/feature/1234", "Absolute ref");
		}

		[TestMethod()]
		public void ShouldReturnNullRelativeRefs()
		{
			Assert.IsNull(Ref.GetRelativeRef("blah"), "Not a ref");
			Assert.IsNull(Ref.GetRelativeRef(""), "Empty ref");
			Assert.IsNull(Ref.GetRelativeRef(null), "null ref");
		}

		[TestMethod()]
		public void ShouldReturnTypesFromRefs()
		{
			Assert.AreEqual(Ref.GetTypeFromRef("/defect/1234"), "defect", "Relative ref");
			Assert.AreEqual(Ref.GetTypeFromRef("/defect/1234.js"), "defect", "Relative ref with extension");
			Assert.AreEqual(Ref.GetTypeFromRef("https://rally1.rallydev.com/slm/webservice/1.32/defect/1234"), "defect", "Valid absolute ref");
		}

		[TestMethod()]
		public void ShouldReturnTypesFromDynatypeRefs()
		{
			Assert.AreEqual(Ref.GetTypeFromRef("/portfolioitem/feature/1234"), "portfolioitem/feature", "Relative ref");
			Assert.AreEqual(Ref.GetTypeFromRef("/portfolioitem/feature/1234.js"), "portfolioitem/feature", "Relative ref with extension");
			Assert.AreEqual(Ref.GetTypeFromRef("https://rally1.rallydev.com/slm/webservice/1.32/portfolioitem/feature/1234"), "portfolioitem/feature", "Valid absolute ref");
		}

		[TestMethod()]
		public void ShouldReturnNullTypesFromRefs()
		{
			Assert.IsNull(Ref.GetTypeFromRef("blah"), "Not a ref");
			Assert.IsNull(Ref.GetTypeFromRef(""), "Empty ref");
			Assert.IsNull(Ref.GetTypeFromRef(null), "null ref");
		}

		[TestMethod()]
		public void ShouldReturnOidsFromRefs()
		{
			Assert.AreEqual(Ref.GetOidFromRef("/defect/1234"), "1234", "Relative ref");
			Assert.AreEqual(Ref.GetOidFromRef("/defect/1234.js"), "1234", "Relative ref with extension");
			Assert.AreEqual(Ref.GetOidFromRef("/typedefinition/-1234.js"), "-1234", "Relative built-in typedef ref with extension");
			Assert.AreEqual(Ref.GetOidFromRef("https://rally1.rallydev.com/slm/webservice/1.32/defect/1234"), "1234", "Valid absolute ref");
		}

		[TestMethod()]
		public void ShouldReturnOidsFromDynatypeRefs()
		{
			Assert.AreEqual(Ref.GetOidFromRef("/portfolioitem/feature/1234"), "1234", "Relative ref");
			Assert.AreEqual(Ref.GetOidFromRef("/portfolioitem/feature/1234.js"), "1234", "Relative ref with extension");
			Assert.AreEqual(Ref.GetOidFromRef("https://rally1.rallydev.com/slm/webservice/1.32/portfolioitem/feature/1234"), "1234", "Valid absolute ref");
		}

		[TestMethod()]
		public void ShouldReturnNullOidsFromRefs()
		{
			Assert.IsNull(Ref.GetOidFromRef("blah"), "Not a ref");
			Assert.IsNull(Ref.GetOidFromRef(""), "Empty ref");
			Assert.IsNull(Ref.GetOidFromRef(null), "null ref");
		}

		[TestMethod()]
		public void ShouldSupportWsapiVersionXinRefs()
		{
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/x/portfolioitem/feature/1234"), "/portfolioitem/feature/1234", "Valid absolute version x dynatype ref");
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/x/defect/1234"), "/defect/1234", "Valid absolute version x ref");
		}

		[TestMethod()]
		public void ShouldSupportWorkspacePermissionRefs()
		{
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/1.38/workspacepermission/123u456w1"), "/workspacepermission/123u456w1", "Valid workspace permission ref");
			Assert.AreEqual(Ref.GetOidFromRef("/workspacepermission/123u456w1.js"), "123u456w1", "Get oid from workspace permission ref");
			Assert.AreEqual(Ref.GetTypeFromRef("/workspacepermission/123u456w1.js"), "workspacepermission", "Get type from workspace permission ref");
		}

		[TestMethod()]
		public void ShouldSupportProjectPermissionRefs()
		{
			// Note: Although this looks like an OID, it is actually a compound string consisting of [UserOID]u[ProjectOID]p[PermissionLevel]
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/1.38/projectpermission/123u456p1"), "/projectpermission/123u456p1", "Valid project permission ref");
			Assert.AreEqual(Ref.GetOidFromRef("/projectpermission/123u456p1.js"), "123u456p1", "Get oid from project permission ref");
			Assert.AreEqual(Ref.GetTypeFromRef("/projectpermission/123u456p1.js"), "projectpermission", "Get type from project permission ref");
		}

		[TestMethod()]
		public void ShouldSupportCollectionRefs()
		{
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/1.38/defect/1234/tasks"), "/defect/1234/tasks", "Valid collection ref");
			Assert.AreEqual(Ref.GetRelativeRef("/defect/1234/tasks"), "/defect/1234/tasks", "Valid relative collection ref");
			Assert.AreEqual(Ref.GetRelativeRef("https://rally1.rallydev.com/slm/webservice/1.38/portfolioitem/feature/1234/children"), "/portfolioitem/feature/1234/children", "Valid dynatype collection ref");
			Assert.AreEqual(Ref.GetRelativeRef("/portfolioitem/feature/1234/children"), "/portfolioitem/feature/1234/children", "Valid dynatype relative collection ref");
			Assert.AreEqual(Ref.GetRelativeRef("/attributedefinition/-12345/allowedvalues"), "/attributedefinition/-12345/allowedvalues", "Valid negative oid collection ref");
		}
	}
}

