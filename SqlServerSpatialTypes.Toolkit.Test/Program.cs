using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using System.Windows.Forms;
using SqlServerSpatialTypes.Toolkit.Viewers;

namespace SqlServerSpatialTypes.Toolkit.Test
{

	static class Program
	{
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FrmTest());
		}
	}

}
