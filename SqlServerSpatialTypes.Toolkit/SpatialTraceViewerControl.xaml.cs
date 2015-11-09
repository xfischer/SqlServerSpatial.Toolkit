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
using SqlServerSpatialTypes.Toolkit.Viewers;
using System.Diagnostics;

namespace SqlServerSpatialTypes.Toolkit
{
	/// <summary>
	/// Logique d'interaction pour SpatialTraceViewerControl.xaml
	/// </summary>
	public partial class SpatialTraceViewerControl : UserControl
	{
		public SpatialTraceViewerControl()
		{
			InitializeComponent();
		}

		string _traceFileName;
		ObservableCollection<TraceLineDesign> _traceLines = null;
		FileSystemWatcher _fsw = null;
		DateTime _lastCheck = DateTime.MinValue;
		private bool _autoDraw; // when FileSystemWatcher raise event, redraw everything

		private string _filePath;
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

				if (_fsw != null)
				{
					FileSystemWatcher_DetachEvents();
					_fsw.EnableRaisingEvents = false;
				}

				_fsw = new FileSystemWatcher(_filePath, SpatialTrace.TraceDataDirectoryName);
				_fsw.IncludeSubdirectories = false;
				_fsw.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
				FileSystemWatcher_AttachEvents();


				_fsw.EnableRaisingEvents = true;

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

		}

		private void FileSystemWatcher_AttachEvents()
		{
			if (_fsw == null)
			{
				Trace.TraceWarning("FileSystemWatcher_AttachEvents: FileSystemWatcher is null.");
				return;
			}
			_fsw.Changed += FileSystemWatcher_EventHandler;
			_fsw.Created += FileSystemWatcher_EventHandler;
		}
		private void FileSystemWatcher_DetachEvents()
		{
			if (_fsw == null)
			{
				Trace.TraceWarning("FileSystemWatcher_DetachEvents: FileSystemWatcher is null.");
				return;
			}
			_fsw.Changed -= FileSystemWatcher_EventHandler;
			_fsw.Created -= FileSystemWatcher_EventHandler;
		}

		public void Close()
		{
			if (_fsw != null)
			{
				_fsw.EnableRaisingEvents = false;
				FileSystemWatcher_DetachEvents();
			}
		}

		void FileSystemWatcher_EventHandler(object sender, FileSystemEventArgs e)
		{
			//Trace.WriteLine(DateTime.Now.ToShortTimeString() + " FileSystemWatcher_EventHandler : " + e.ChangeType.ToString());

			if ((DateTime.Now - _lastCheck).TotalMilliseconds > 250)
			{
				int currentCount = _traceLines.Count;
				int fileCount = 0;
				using (FileStream fs = new FileStream(_traceFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (StreamReader sr = new StreamReader(fs))
					{
						string lineText = sr.ReadLine(); // skip header
						lineText = sr.ReadLine();
						while (lineText != null)
						{
							fileCount++;
							if (fileCount > currentCount)
							{
								TraceLineDesign traceLine = TraceLineDesign.Parse(lineText);
								this.Dispatcher.BeginInvoke((Action)(() => { _traceLines.Add(traceLine); if (_autoDraw) lvTrace.SelectedItems.Add(traceLine); }));
							}
							lineText = sr.ReadLine();

						}
					}
				}

				_lastCheck = DateTime.Now;
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
							listGeom.Add(SqlGeomStyledFactory.Create(SqlTypesExtensions.Read(System.IO.Path.Combine(_filePath, trace.GeometryDataFile)),trace.Message, trace.FillColor, trace.StrokeColor, trace.StrokeWidth));
						}
					}

				}

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

		private void chkAutoDraw_Click(object sender, RoutedEventArgs e)
		{
			_autoDraw = chkAutoDraw.IsChecked.Value;
		}
	}
}

