using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlServerSpatialTypes.Toolkit.Viewers
{
	public partial class FrmViewer : Form
	{
		public FrmViewer()
		{
			InitializeComponent();

			this.Shown += FrmViewer_Shown;
		}

		public ISpatialViewer Viewer
		{
			get { return spatialViewerControl1; }
		}

		protected override void OnClosed(EventArgs e)
		{
			this.Shown -= FrmViewer_Shown;
			base.OnClosed(e);
		}

		void FrmViewer_Shown(object sender, EventArgs e)
		{
			spatialViewerControl1.ResetView();

		}
	}
}
