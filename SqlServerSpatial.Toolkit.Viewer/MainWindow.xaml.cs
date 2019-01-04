using Microsoft.Win32;
using NetTopologySuite.Diagnostics.Tracing;
using NetTopologySuite.Diagnostics.Viewers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NetTopologySuite.Diagnostics.Viewer
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.AllowDrop = true;
			this.Drop += MainWindow_Drop;

#if DEBUG
			DebugPanel.Visibility = Visibility.Visible;
			viewer.Visibility = Visibility.Visible;
#else
			DebugPanel.Visibility = Visibility.Collapsed;
			viewer.Visibility = Visibility.Collapsed;
#endif


		}

		void MainWindow_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				// Assuming you have one file that you care about, pass it off to whatever
				// handling code you have defined.
				LaunchTraceViewer(files[0]);
				e.Handled = true;
			}
		}

	

		private void btnTraceView_Click(object sender, RoutedEventArgs e)
		{
			TestTraceViewer();
		}
		private void ResetViewButton_Click(object sender, RoutedEventArgs e)
		{
			((ISpatialViewer)viewer).ResetView();
		}

		private void TestTraceViewer()
		{
			string traceFilePath = SpatialTrace.TraceFilePath;
			if (traceFilePath == null)
			{
				MessageBox.Show("No current trace");
				return;
			}
			LaunchTraceViewer(traceFilePath);
		}

		private void LaunchTraceViewer(string traceFilePath)
		{
			SpatialTraceViewerControl ctlTraceViewer = new SpatialTraceViewerControl();
			ctlTraceViewer.Initialize(traceFilePath);
			Window wnd = new Window();
			wnd.Title = "NetTopologySuite Diagnostics Viewer";
			wnd.Content = ctlTraceViewer;
			wnd.Closed += (o, e) => { ctlTraceViewer.Dispose(); };
			wnd.Show();
		}


		private void btnTraceLoad_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Trace files (SpatialTrace*.txt)|SpatialTrace*.txt";
			if (dlg.ShowDialog().GetValueOrDefault(false))
			{
				LaunchTraceViewer(dlg.FileName);
			}
		}
        
    }
}
