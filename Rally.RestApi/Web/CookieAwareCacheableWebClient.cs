using Rally.RestApi.Exceptions;
using Rally.RestApi.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Rally.RestApi.Web
{
	[System.ComponentModel.DesignerCategory("")]
	internal class CookieAwareCacheableWebClient : CookieAwareWebClient
	{
		/// <summary>
		/// The file location where data is stored.
		/// </summary>
		private static DirectoryInfo fileDirectory;

		private readonly DynamicJsonSerializer serializer = new DynamicJsonSerializer();
		[Serializable]
		private class CachedResult
		{
			public string Url { get; set; }
			public DynamicJsonObject ResponseData { get; set; }

			public CachedResult(string redirectUrl, DynamicJsonObject responseData)
			{
				Url = redirectUrl;
				ResponseData = responseData;
			}
		}

		// Tracking Key: Username|SourceUrl
		private static Dictionary<string, CachedResult> cachedResults;
		private static object dataLock;
		private CachedResult returnValue = null;
		private bool isCachedResult = false;

		#region Constructor
		static CookieAwareCacheableWebClient()
		{
			cachedResults = new Dictionary<string, CachedResult>();
			dataLock = new object();

			fileDirectory = new DirectoryInfo(String.Format(@"{0}\Rally\DotNetRestApi",
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));
			if (!fileDirectory.Exists)
				fileDirectory.Create();

			ClearOldCacheFilesFromDisk();
		}

		public CookieAwareCacheableWebClient(CookieContainer cookies = null)
			: base(cookies)
		{
		}
		#endregion

		#region GetWebRequest
		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			HttpWebRequest webRequest = request as HttpWebRequest;
			if (webRequest != null)
			{
				// IMPORTANT: 
				// Don't allow redirect as it prevents us from disovering if we 
				// are using a cached endpoint, or have a new result set.
				webRequest.AllowAutoRedirect = false;
			}

			return request;
		}
		#endregion

		#region DownloadCacheableResult
		/// <summary>
		/// Downloads the requested resource as a System.String. The resource to download is 
		/// specified as a System.String containing the URI.
		/// </summary>
		/// <param name="address">A System.String containing the URI to download.</param>
		/// <param name="isCachedResult">If the returned result was a cached result.</param>
		/// <returns>A System.String containing the requested resource.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		public DynamicJsonObject DownloadCacheableResult(string address, out bool isCachedResult)
		{
			return DownloadCacheableResult(new Uri(address), out isCachedResult);
		}

		/// <summary>
		/// Downloads the requested resource as a System.String. The resource to 
		/// download is specified as a System.Uri.
		/// </summary>
		/// <param name="address">A System.Uri object containing the URI to download.</param>
		/// <param name="isCachedResult">If the returned result was a cached result.</param>
		/// <returns>A System.String containing the requested resource.</returns>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		public DynamicJsonObject DownloadCacheableResult(Uri address, out bool isCachedResult)
		{
			string results = DownloadString(address);

			isCachedResult = this.isCachedResult;
			if (returnValue != null)
				return returnValue.ResponseData;

			return serializer.Deserialize(results);
		}
		#endregion

		#region GetWebResponse
		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse response = base.GetWebResponse(request);
			HttpWebResponse webResponse = response as HttpWebResponse;
			if (webResponse != null)
			{
				// Check to see if it's a redirect
				if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
				{
					string uriString = webResponse.Headers["Location"];
					string cachingKey = String.Empty;
					if (request.Credentials != null)
					{
						NetworkCredential credential = request.Credentials.GetCredential(request.RequestUri, "Basic");
						cachingKey = credential.UserName;
					}
					else
					{
						CookieCollection cookieCollection = Cookies.GetCookies(request.RequestUri);
						foreach (Cookie currentCookie in cookieCollection)
						{
							if (currentCookie.Name.Equals(RallyRestApi.ZSessionID, StringComparison.InvariantCultureIgnoreCase))
							{
								cachingKey = currentCookie.Value;
								break;
							}
						}
					}

					returnValue = GetCachedResult(cachingKey, request.RequestUri.ToString());
					if ((returnValue == null) || (!returnValue.Url.Equals(uriString)))
					{
						if (returnValue != null)
						{
							ClearCacheResult(cachingKey, request.RequestUri.ToString());
							returnValue = null;
						}

						CookieAwareWebClient webClient = GetWebClient();

						string cacheableDataValue = webClient.DownloadString(uriString);
						returnValue = CacheResult(cachingKey, request.RequestUri.ToString(), uriString, serializer.Deserialize(cacheableDataValue));
					}
					else
						isCachedResult = true;
				}
			}

			return response;
		}
		#endregion

		#region GetWebClient
		private CookieAwareWebClient GetWebClient()
		{
			CookieAwareWebClient webClient = new CookieAwareWebClient(Cookies);

			foreach (string key in webClient.Headers.Keys)
				webClient.Headers.Add(key, webClient.Headers[key]);

			webClient.Encoding = Encoding;
			webClient.Credentials = Credentials;

			if (Proxy != null)
			{
				webClient.Proxy = Proxy;
			}

			return webClient;
		}
		#endregion

		#region CacheResult
		private CachedResult CacheResult(string userName, string sourceUrl, string redirectUrl, DynamicJsonObject responseData)
		{
			string cacheKey = GetCacheKey(userName, sourceUrl);
			CachedResult cachedResult = new CachedResult(redirectUrl, responseData);
			lock (dataLock)
			{
				if (cachedResults.ContainsKey(cacheKey))
					cachedResults[cacheKey] = cachedResult;
				else
					cachedResults.Add(cacheKey, cachedResult);


				FileInfo fileLoction = GetFileLocation(cacheKey);
				File.WriteAllBytes(fileLoction.FullName, SerializeData(cachedResult));
			}

			return cachedResult;
		}
		#endregion

		#region GetCachedResult
		private CachedResult GetCachedResult(string userName, string sourceUrl)
		{
			string cacheKey = GetCacheKey(userName, sourceUrl);
			lock (dataLock)
			{
				if (!cachedResults.ContainsKey(cacheKey))
				{
					// If not in cached results, try loading from disk.
					FileInfo fileLoction = GetFileLocation(cacheKey);
					if (fileLoction.Exists)
					{
						byte[] fileData = File.ReadAllBytes(fileLoction.FullName);
						cachedResults.Add(cacheKey, DeserializeData(fileData));
					}
				}

				if (cachedResults.ContainsKey(cacheKey))
					return cachedResults[cacheKey];
				else
					return null;
			}
		}
		#endregion

		#region ClearCacheResult
		private void ClearCacheResult(string userName, string sourceUrl)
		{
			string cacheKey = GetCacheKey(userName, sourceUrl);
			lock (dataLock)
			{
				if (cachedResults.ContainsKey(cacheKey))
					cachedResults.Remove(cacheKey);
			}
		}
		#endregion

		#region ClearOldCacheFilesFromDisk
		/// <summary>
		/// Deletes all cache files older than 14 days.
		/// </summary>
		internal static void ClearOldCacheFilesFromDisk(bool force = false)
		{
			string[] files = Directory.GetFiles(fileDirectory.FullName, "*.*", SearchOption.AllDirectories);
			foreach (string current in files)
			{
				FileInfo currentFile = new FileInfo(current);
				if ((force) || (currentFile.CreationTime < DateTime.Now.AddDays(-14)))
				{
					try
					{
						File.Delete(current);
					}
					catch { }
				}
			}
		}
		#endregion

		#region GetCacheKey
		private string GetCacheKey(string userName, string sourceUrl)
		{
			return String.Format("{0}|{1}", userName, sourceUrl);
		}
		#endregion

		#region GetFileLocation
		/// <summary>
		/// Gets the file location to save to within the data cache.
		/// </summary>
		private static FileInfo GetFileLocation(string cacheKey)
		{
			string hash = ComputeHash(cacheKey);
			string fileName = String.Format(@"{0}\{1}.data", fileDirectory.FullName, hash.Replace("/", ""));
			return new FileInfo(fileName);
		}
		#endregion

		#region ComputeHash
		/// <summary>Generates a hash for the given plain text value and returns a base64-encoded result.</summary>
		/// <param name="textToHash">Plaintext value to be hashed.</param>
		private static string ComputeHash(string textToHash)
		{
			if (textToHash == null)
				textToHash = String.Empty;

			byte[] textBytes = Encoding.UTF8.GetBytes(textToHash);
			HashAlgorithm hash = new SHA512Managed();
			byte[] hashBytes = hash.ComputeHash(textBytes);
			return Convert.ToBase64String(hashBytes);
		}
		#endregion

		#region Serialize Data
		/// <summary>
		/// Serializes data to be sent across the wire.
		/// <para>Serialization errors of lists may be caused by a missing constructor with the following signature:
		/// public [CLASS NAME](SerializationInfo info, StreamingContext context)
		/// </para>
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <returns>The serialized data.</returns>
		private static byte[] SerializeData(CachedResult obj)
		{
			if (obj == null)
				return null;

			BinaryFormatter binFor = new BinaryFormatter();
			byte[] data = null;
			using (MemoryStream stream = new MemoryStream())
			{
				binFor.Serialize(stream, obj);
				data = stream.ToArray();
			}

			return data;
		}
		#endregion

		#region Deserialize Data
		/// <summary>
		/// Deserializes the data from the service back into its original object type.
		/// <para>Deserialization errors of lists may be caused by a missing constructor with the following signature:
		/// public [CLASS NAME](SerializationInfo info, StreamingContext context)
		/// </para>
		/// </summary>
		/// <param name="serializedData">The serialized data.</param>
		/// <returns>The deserialized object.</returns>
		private static CachedResult DeserializeData(byte[] serializedData)
		{
			if (serializedData == null)
				return null;

			CachedResult typedObj;
			BinaryFormatter binFor = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream(serializedData))
			{
				object obj = binFor.Deserialize(stream);
				if (obj is CachedResult)
					typedObj = (CachedResult)obj;
				else
					throw new InvalidDataException("Data sent to deserialization is not of type DynamicJsonObject.");
			}
			return typedObj;
		}
		#endregion
	}
}
