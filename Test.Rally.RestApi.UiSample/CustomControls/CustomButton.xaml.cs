using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test.Rally.RestApi.UiSample.CustomControls
{
	/// <summary>
	/// Interaction logic for CustomButton.xaml
	/// </summary>
	public partial class CustomButton : Button
	{
		public CustomButton()
		{
			InitializeComponent();
			Background = Brushes.DarkRed;
			Foreground = Brushes.White;
		}
	}
}
