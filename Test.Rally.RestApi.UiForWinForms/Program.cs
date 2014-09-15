using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Rally.RestApi.UiForWinForms
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//Change SSL checks so that all checks pass
			ServicePointManager.ServerCertificateValidationCallback =
					new RemoteCertificateValidationCallback(delegate { return true; });

			Application.Run(new MainWindow());
		}
	}
}
