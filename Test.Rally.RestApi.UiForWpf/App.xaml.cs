﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows;

namespace Test.Rally.RestApi.UiForWpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			//Change SSL checks so that all checks pass
			ServicePointManager.ServerCertificateValidationCallback =
					new RemoteCertificateValidationCallback(delegate { return true; });
		}
	}
}
