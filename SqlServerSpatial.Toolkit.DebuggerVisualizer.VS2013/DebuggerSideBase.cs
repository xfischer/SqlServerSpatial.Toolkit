﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.DebuggerVisualizers;
using SqlServerSpatial.Toolkit.Viewers;

namespace SqlServerSpatial.Toolkit.Visualizer
{
    public abstract class DebuggerSideBase : DialogDebuggerVisualizer
	{

		protected abstract SqlGeometry GetObject(IVisualizerObjectProvider objectProvider);

		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{

			try
			{
				SqlGeometry geometry = GetObject(objectProvider);

				using (FrmGeometryViewer frmViewer = new FrmGeometryViewer())
				{
					frmViewer.Viewer.SetGeometry(new SqlGeometryStyled(geometry, null, Color.FromArgb(200, 0, 175, 0), Colors.Black, 1f));

					// Show the grid with the list
					windowService.ShowDialog(frmViewer);
				}
			}
			catch (Exception e)
			{
				System.Windows.MessageBox.Show("Error: " + e.ToString());
			}
		}
	}

	public abstract class DebuggerSideListBase : DialogDebuggerVisualizer
	{

		protected abstract IEnumerable<SqlGeometry> GetObject(IVisualizerObjectProvider objectProvider);

		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{

			try
			{
				IEnumerable<SqlGeometry> geometry = GetObject(objectProvider);

				using (FrmGeometryViewer frmViewer = new FrmGeometryViewer())
				{
					SqlGeomStyledFactory.Create(geometry, null, Color.FromArgb(200, 0, 175, 0), Colors.Black, 1f);

					// Show the grid with the list
					windowService.ShowDialog(frmViewer);
				}
			}
			catch (Exception e)
			{
				System.Windows.MessageBox.Show("Error: " + e.ToString());
			}
		}
	}
}