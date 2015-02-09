using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Rally.RestApi.UiSample
{
	public class EncryptionUtilities : IEncryptionRoutines
	{
		public string EncryptString(string keyString, string textToEncrypt)
		{
			return textToEncrypt;
		}

		public string DecryptString(string keyString, string textToEncrypt)
		{
			return textToEncrypt;
		}
	}
}
