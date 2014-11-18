using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rally.RestApi.UiForWpf
{
	/// <summary>
	/// A list of control types that can be replaced by custom controls. 
	/// </summary>
	public enum CustomWpfControlType
	{
		/// <summary>
		/// Must extend from Button.
		/// </summary>
		Buttons,
		/// <summary>
		/// Must extend from Selector
		/// </summary>
		TabControl,
		/// <summary>
		/// Must extend from HeaderedContentControl
		/// </summary>
		TabItem,
	}
}
