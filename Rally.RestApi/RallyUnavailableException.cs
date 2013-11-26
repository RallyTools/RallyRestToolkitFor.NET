using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi
{
	/// <summary>
	/// An exception indicating that Rally is temporarily offline.
	/// </summary>
	public class RallyUnavailableException : Exception
	{
	}
}
