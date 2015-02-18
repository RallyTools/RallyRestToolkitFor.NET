using Rally.RestApi.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Xml;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// A class for storing a users login details.
	/// </summary>
	public class LoginDetails
	{
		#region Properties and Fields
		/// <summary>
		/// The URL for the Rally Server that we are connecting to.
		/// </summary>
		public string RallyServer { get; set; }
		/// <summary>
		/// The Rally username that we are connecting as.
		/// </summary>
		public string Username { get; set; }
		/// <summary>
		/// The ZSessionID for this user.
		/// </summary>
		public string ZSessionID { get; set; }
		/// <summary>
		/// Proxy server url
		/// </summary>
		public string ProxyServer { get; set; }
		/// <summary>
		/// Proxy server user name
		/// </summary>
		public string ProxyUsername { get; set; }
		/// <summary>
		/// The URL for the IDP Server that we are connecting to (IDP based connections only).
		/// </summary>
		public string IdpServer { get; set; }
		/// <summary>
		/// ConnectionType enum value determining SSO auth type
		/// </summary>
		public ConnectionType ConnectionType { get; set; }
		bool trustAllCertificates;
		/// <summary>
		/// Should all certificates be trusted?
		/// </summary>
		public bool TrustAllCertificates
		{
			get { return trustAllCertificates; }
			set
			{
				trustAllCertificates = value;
				if (TrustAllCertificates)
				{
					ServicePointManager.ServerCertificateValidationCallback =
						new RemoteCertificateValidationCallback(delegate { return true; });
				}
				else
					ServicePointManager.ServerCertificateValidationCallback = null;
			}
		}

		private string password = null;
		private string proxyPassword = null;
		private ApiAuthManager authMgr;
		private static DirectoryInfo fileDirectory;
		#endregion

		#region LoginDetails
		static LoginDetails()
		{
			fileDirectory = new DirectoryInfo(String.Format(@"{0}\Rally\RallyRestAPI",
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));
			if (!fileDirectory.Exists)
				fileDirectory.Create();
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="authMgr">The authorization manager that owns these login details.</param>
		public LoginDetails(ApiAuthManager authMgr)
		{
			ConnectionType = ConnectionType.BasicAuth;
			this.authMgr = authMgr;
		}
		#endregion

		#region Set/GetPassword
		/// <summary>
		/// Sets the password with encryption for this user.
		/// </summary>
		/// <param name="password">The password to set.</param>
		public void SetPassword(string password)
		{
			this.password = authMgr.EncryptionRoutines.EncryptString(authMgr.EncryptionKey, password);
		}
		/// <summary>
		/// Gets the decrypted password for this user.
		/// </summary>
		/// <returns>The password after it was decrypted.</returns>
		public string GetPassword()
		{
			return authMgr.EncryptionRoutines.DecryptString(authMgr.EncryptionKey, password);
		}
		/// <summary>
		/// Sets the proxy password with encryption for this user.
		/// </summary>
		/// <param name="password">The password to set.</param>
		public void SetProxyPassword(string password)
		{
			this.proxyPassword = authMgr.EncryptionRoutines.EncryptString(authMgr.EncryptionKey, password);
		}
		/// <summary>
		/// Gets the decrypted proxy password for this user.
		/// </summary>
		/// <returns>The password after it was decrypted.</returns>
		public string GetProxyPassword()
		{
			return authMgr.EncryptionRoutines.DecryptString(authMgr.EncryptionKey, proxyPassword);
		}
		#endregion

		#region LoadFromDisk
		internal void LoadFromDisk()
		{
			FileInfo fileOnDisk = GetFileOnDisk();
			if (fileOnDisk == null)
				return;

			XmlDocument xmlDoc = new XmlDocument();
			if (fileOnDisk.Exists)
			{
				xmlDoc.Load(fileOnDisk.FullName);

				RallyServer = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "RallyServer");
				Username = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "Username");
				ZSessionID = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "ZSessionID");
				IdpServer = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "IdpServer");
				string connType = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "ConnectionType");
				ConnectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), connType);

				string trustAllCerts = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "TrustAllCertificates");
				Boolean.TryParse(trustAllCerts, out trustAllCertificates);

				string passwordValue = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "Password");
				if (!String.IsNullOrWhiteSpace(passwordValue))
					password = passwordValue;
				else
					SetPassword(String.Empty);

				ProxyServer = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "ProxyServer");
				ProxyUsername = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "ProxyUsername");

				passwordValue = GetChildNodeValue(xmlDoc.FirstChild.ChildNodes, "ProxyPassword");
				if (!String.IsNullOrWhiteSpace(passwordValue))
					proxyPassword = passwordValue;
				else
					SetProxyPassword(String.Empty);
			}
		}
		#endregion

		#region SaveToDisk
		internal void SaveToDisk()
		{
			FileInfo fileOnDisk = GetFileOnDisk();
			if (!fileOnDisk.Directory.Exists)
				fileOnDisk.Directory.Create();

			XmlDocument xmlDoc = new XmlDocument();
			XmlElement rootNode = xmlDoc.CreateElement("LoginDetails");
			xmlDoc.AppendChild(rootNode);

			AddChildNode(rootNode, "RallyServer", RallyServer);
			AddChildNode(rootNode, "Username", Username);
			AddChildNode(rootNode, "ZSessionID", ZSessionID);
			AddChildNode(rootNode, "IdpServer", IdpServer);
			AddChildNode(rootNode, "ConnectionType", ConnectionType.ToString());
			AddChildNode(rootNode, "TrustAllCertificates", trustAllCertificates.ToString());
			if (!String.IsNullOrWhiteSpace(GetPassword()))
				AddChildNode(rootNode, "Password", password);
			else
				AddChildNode(rootNode, "Password", String.Empty);

			AddChildNode(rootNode, "ProxyServer", ProxyServer);
			AddChildNode(rootNode, "ProxyUsername", ProxyUsername);
			if (!String.IsNullOrWhiteSpace(GetProxyPassword()))
				AddChildNode(rootNode, "ProxyPassword", proxyPassword);
			else
				AddChildNode(rootNode, "ProxyPassword", String.Empty);

			try
			{
				xmlDoc.Save(fileOnDisk.FullName);
			}
			catch
			{
			}
		}
		#endregion

		#region DeleteCachedLoginDetailsFromDisk
		/// <summary>
		/// Deletes any cached login credentials from disk.
		/// </summary>
		/// <returns>If the files were successfully deleted or not.</returns>
		internal bool DeleteCachedLoginDetailsFromDisk()
		{
			FileInfo fileOnDisk = GetFileOnDisk();
			if (fileOnDisk.Exists)
			{
				try
				{
					File.Delete(fileOnDisk.FullName);
					return true;
				}
				catch
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region GetFileOnDisk
		private FileInfo GetFileOnDisk()
		{
			return new FileInfo(String.Format(@"{0}\{1}.xml", fileDirectory.FullName, authMgr.ApplicationToken));
		}
		#endregion

		#region GetChildNodeValue
		/// <summary>
		/// Gets the value from a child node.
		/// </summary>
		private string GetChildNodeValue(XmlNodeList childNodes, string childNodeName)
		{
			if (childNodes == null)
				return null;

			foreach (XmlElement node in childNodes)
			{
				if (node.Name.Equals(childNodeName, StringComparison.InvariantCultureIgnoreCase))
					return node.InnerXml;
			}

			return null;
		}
		#endregion

		#region AddChildNode
		/// <summary>
		/// Adds a child node to a parent node.
		/// </summary>
		private XmlElement AddChildNode(XmlElement parentNode, string name, string dataValue)
		{
			XmlElement node = parentNode.OwnerDocument.CreateElement(name);
			if (!String.IsNullOrWhiteSpace(dataValue))
				node.InnerText = dataValue;

			parentNode.AppendChild(node);
			return node;
		}
		#endregion

		#region MarkUserAsLoggedOut
		internal void MarkUserAsLoggedOut()
		{
			Username = String.Empty;
			password = String.Empty;
			SaveToDisk();
		}
		#endregion
	}
}
