using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlServerSpatial.Toolkit
{
	public partial class FrmTraceViewer : Form
	{
		public FrmTraceViewer()
		{
			InitializeComponent();
			this.Shown += FrmViewer_Shown;
		}

		public ISpatialViewer Viewer
		{
			get { return spatialTraceViewerControl1.viewer; }
		}

		private string _traceFileName;
		internal void Initialize(string traceFileName)
		{
			_traceFileName = traceFileName;
		}

		protected override void OnClosed(EventArgs e)
		{
			this.Shown -= FrmViewer_Shown;
			this.spatialTraceViewerControl1.Dispose();
			base.OnClosed(e);
		}

		void FrmViewer_Shown(object sender, EventArgs e)
		{
			// Initialize called here because at this the the layout is ready and columns can be autosized
			if (_traceFileName != null)
				spatialTraceViewerControl1.Initialize(_traceFileName);
			_traceFileName = null;

			spatialTraceViewerControl1.viewer.ResetView();
		}
	}
}
