using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// An interface for encrypting and decrypting passwords.
	/// </summary>
	public interface IEncryptionRoutines
	{
		/// <summary>
		/// Encrypts a string using the provided key string as a salting token.
		/// </summary>
		/// <param name="keyString">The key string, or salt, to use when encrypting.</param>
		/// <param name="textToEncrypt">The text to be encrypted.</param>
		/// <returns>The encrypted string.</returns>
		string EncryptString(string keyString, string textToEncrypt);
		/// <summary>
		/// Decrypts a string using the provided key string as a decryption token.
		/// </summary>
		/// <param name="keyString">The key string, or salt, to use when decrypting.</param>
		/// <param name="textToDecrypt">The text to be decrypted.</param>
		/// <returns>The decrypted string.</returns>
		string DecryptString(string keyString, string textToDecrypt);
	}
}
