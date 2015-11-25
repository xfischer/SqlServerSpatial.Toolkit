using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SqlServerSpatial.Toolkit.Viewers;
using System.Diagnostics;

namespace SqlServerSpatial.Toolkit
{
	/// <summary>
	/// Spatial trace viewer control
	/// </summary>
	public partial class SpatialTraceViewerControl : UserControl, IDisposable
	{
		readonly IClipboardHandler _clipboardHandler = new ClipboardHandler();

		/// <summary>
		/// Public constructor
		/// </summary>
		public SpatialTraceViewerControl()
		{
			InitializeComponent();

			viewer.GetSQLSourceText += viewer_GetSQLSourceText;
		}

		#region Source SQL Text

		void viewer_GetSQLSourceText(object sender, EventArgs e)
		{
			_clipboardHandler.SetClipboardText();
		}

		#endregion

		string _traceFileName;
		ObservableCollection<TraceLineDesign> _traceLines = null;

		private string _filePath;
		/// <summary>
		/// Load the specified spatial trace file
		/// </summary>
		/// <param name="traceFileName">SpatialTrace.txt file generated in traced assembly binaries directory.</param>
		public void Initialize(string traceFileName)
		{
			try
			{
				_traceFileName = traceFileName;
				_filePath = System.IO.Path.GetDirectoryName(_traceFileName);

				_traceLines = new ObservableCollection<TraceLineDesign>();
				using (FileStream fs = new FileStream(_traceFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (StreamReader sr = new StreamReader(fs))
					{
						string lineText = sr.ReadLine(); // skip header
						lineText = sr.ReadLine();
						while (lineText != null)
						{
							TraceLineDesign traceLine = TraceLineDesign.Parse(lineText);
							_traceLines.Add(traceLine);
							lineText = sr.ReadLine();
						}
					}
				}

				lvTrace.ItemsSource = _traceLines;

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

		}

		private void lvTrace_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				List<SqlGeometryStyled> listGeom = new List<SqlGeometryStyled>();
				foreach (TraceLineDesign trace in lvTrace.SelectedItems.OfType<TraceLineDesign>().OrderBy(t => t.DateTime))
				{
					if (string.IsNullOrEmpty(trace.GeometryDataFile) == false)
					{
						if (trace.GeometryDataFile.EndsWith("list.dat"))
						{
							listGeom.AddRange(SqlGeomStyledFactory.Create(SqlTypesExtensions.ReadList(System.IO.Path.Combine(_filePath, trace.GeometryDataFile)), trace.Message, trace.FillColor, trace.StrokeColor, trace.StrokeWidth));
						}
						else
						{
							listGeom.Add(SqlGeomStyledFactory.Create(SqlTypesExtensions.Read(System.IO.Path.Combine(_filePath, trace.GeometryDataFile)), trace.Message, trace.FillColor, trace.StrokeColor, trace.StrokeWidth));
						}
					}
				}

				_clipboardHandler.Initialize(listGeom.Select(g => g.Geometry).ToList());
				if (listGeom.Count == 0)
					viewer.Clear();
				else
					viewer.SetGeometry(listGeom);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

		}

		private void btnReload_Click(object sender, RoutedEventArgs e)
		{
			Initialize(_traceFileName);
		}

		/// <summary>
		/// Release all events and resources
		/// </summary>
		public void Dispose()
		{
			viewer.GetSQLSourceText -= viewer_GetSQLSourceText;
		}
	}
}

