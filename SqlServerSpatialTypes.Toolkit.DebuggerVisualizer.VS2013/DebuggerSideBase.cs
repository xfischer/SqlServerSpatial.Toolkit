using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.DebuggerVisualizers;
using SqlServerSpatialTypes.Toolkit.Viewers;

namespace SqlServerSpatialTypes.Toolkit.Visualizer
{
	public abstract class DebuggerSideBase : DialogDebuggerVisualizer
	{

		protected abstract SqlGeometry GetObject(IVisualizerObjectProvider objectProvider);

		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{
			FrmViewer frmViewer = new FrmViewer();
			try
			{
				SqlGeometry geometry = GetObject(objectProvider);

				frmViewer.Viewer.SetGeometry(new SqlGeometryStyled(geometry, null, Color.FromArgb(200, 0, 175, 0), Colors.Black, 1f));

				// Show the grid with the list
				windowService.ShowDialog(frmViewer);
			}
			catch (Exception e)
			{
				System.Windows.MessageBox.Show("Error: " + e.ToString());
			}
		}
	}
}
