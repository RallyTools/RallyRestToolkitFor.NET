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
		/// Replaces the default Button with a custom one. The replacement button must extend from <see cref="System.Windows.Controls.Button"/>.
		/// </summary>
		Buttons,
		/// <summary>
		/// Replaces the default tab control with a custom one. The replacement tab control must extend from <see cref="System.Windows.Controls.Primitives.Selector"/>.
		/// </summary>
		TabControl,
		/// <summary>
		/// Replaces the default tab item with a custom one. The replacement tab item control must extend from <see cref="System.Windows.Controls.HeaderedContentControl"/>.
		/// </summary>
		TabItem,
	}
}
