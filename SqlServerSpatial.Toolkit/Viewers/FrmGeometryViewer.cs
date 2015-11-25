using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit.Viewers;

namespace SqlServerSpatial.Toolkit.Debugging
{
	public partial class FrmGeometryViewer : Form
	{
		readonly IClipboardHandler _clipboardHandler = new ClipboardHandler();

		public FrmGeometryViewer()
		{
			InitializeComponent();

			this.Shown += FrmViewer_Shown;
			this.Viewer.GetSQLSourceText += Viewer_GetSQLSourceText;
		}

		void Viewer_GetSQLSourceText(object sender, EventArgs e)
		{
			_clipboardHandler.SetClipboardText();
		}

		public void SetGeometry(SqlGeometryStyled geometry)
		{
			_clipboardHandler.Initialize(new List<SqlGeometry>() { geometry.Geometry });
			Viewer.SetGeometry(geometry);
		}
		public void SetGeometry(IEnumerable<SqlGeometryStyled> geometries)
		{
			_clipboardHandler.Initialize(geometries.Select(g => g.Geometry));
			Viewer.SetGeometry(geometries);
		}

		private ISpatialViewer Viewer
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
