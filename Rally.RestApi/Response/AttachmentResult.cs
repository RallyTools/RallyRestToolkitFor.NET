using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Response
{
	/// <summary>
	/// A result containing the contents of an attachment.
	/// </summary>
	public class AttachmentResult : OperationResult
	{
		/// <summary>
		/// The file contents that were downloaded.
		/// </summary>
		public byte[] FileContents { get; internal set; }

		internal AttachmentResult()
		{
		}
	}
}
