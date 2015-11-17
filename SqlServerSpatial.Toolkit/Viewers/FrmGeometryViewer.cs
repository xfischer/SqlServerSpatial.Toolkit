using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlServerSpatial.Toolkit.Viewers
{
	public partial class FrmGeometryViewer : Form
	{
		public FrmGeometryViewer()
		{
			InitializeComponent();

			this.Shown += FrmViewer_Shown;
		}

		internal ISpatialViewer Viewer
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
