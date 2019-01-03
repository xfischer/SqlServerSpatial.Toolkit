using GeoAPI.Geometries;
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
using System.ComponentModel;

namespace SqlServerSpatial.Toolkit
{

	/// <summary>
	/// Logique d'interaction pour SpatialTraceViewerControl.xaml
	/// </summary>
	public partial class SpatialTraceViewerControl : UserControl, IDisposable
	{
		ITraceViewModel _viewModel;
		public SpatialTraceViewerControl()
		{
			InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
			{
				_viewModel = new TraceViewModel();
			}
			else
			{
				_viewModel = new TraceViewModel();
			}
			this.DataContext = _viewModel;
						
		}

		public void Dispose()
		{
			
		}

		#region Source SQL Text

		
		void viewer_GetSQLSourceText(object sender, EventArgs e)
		{
			string data = ((ISpatialViewer)viewer).GetSQLSourceText();
			if (data != null) Clipboard.SetText(data);
		}

		

		#endregion

		string _traceFileName;
		ObservableCollection<TraceLineDesign> _traceLines = null;
		Dictionary<int, List<IGeometryStyled>> _listGeometryStyles = new Dictionary<int, List<IGeometryStyled>>();
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
							// Parse line
							TraceLineDesign traceLine = TraceLineDesign.Parse(lineText);

							// Create geometry
							if (string.IsNullOrWhiteSpace(traceLine.GeometryDataFile) == false)
							{
								if (traceLine.GeometryDataFile.EndsWith("list.dat"))
								{
									_listGeometryStyles[traceLine.UniqueId] = SqlGeomStyledFactory.Create(SqlTypesExtensions.ReadList(System.IO.Path.Combine(_filePath, traceLine.GeometryDataFile)),
																																												traceLine.FillColor,
																																												traceLine.StrokeColor,
																																												traceLine.StrokeWidth,
																																												traceLine.Label,
																																												traceLine.IsChecked);
								}
								else
								{
									_listGeometryStyles[traceLine.UniqueId] = new List<IGeometryStyled>() { SqlGeomStyledFactory.Create(SqlTypesExtensions.Read(System.IO.Path.Combine(_filePath, traceLine.GeometryDataFile)),
																																												traceLine.FillColor,
																																												traceLine.StrokeColor,
																																												traceLine.StrokeWidth,
																																												traceLine.Label,
																																												traceLine.IsChecked) };
								}
							}

							// Add to collection
							_traceLines.Add(traceLine);

							// next value
							lineText = sr.ReadLine();
						}
					}
				}

					
				

				// Set to view model
				_viewModel.Traces = _traceLines;
				
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
				List<IGeometryStyled> listGeom = new List<IGeometryStyled>();
				HashSet<TraceLineDesign> listTrace2Draw = new HashSet<TraceLineDesign>(lvTrace.SelectedItems.OfType<TraceLineDesign>());
				listTrace2Draw.UnionWith(_traceLines.Where(t => t.IsChecked));
				IEnumerable<int> v_listId2Draw = listTrace2Draw.OrderBy(t => t.DateTime).Select(t => t.UniqueId).Intersect(_listGeometryStyles.Keys);

				foreach (TraceLineDesign trace in listTrace2Draw.OrderBy(t => t.DateTime))
				{
					listGeom.AddRange(_listGeometryStyles[trace.UniqueId]);
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


		private void SelectCheckBox_Click(object sender, RoutedEventArgs e)
		{
			UpdateCheckboxes(sender as CheckBox);
		}

		private void UpdateCheckboxes(CheckBox checkBox)
		{
			if (checkBox != null)
			{
				// Get which checkbox was click looking at its tag
				bool v_isCheck = checkBox.Tag.ToString() == "Select";
				// Revert back to initial checked state
				checkBox.IsChecked = v_isCheck;

				// change model accordingly
				foreach (TraceLineDesign trace in lvTrace.SelectedItems.OfType<TraceLineDesign>())
				{
					trace.IsChecked = v_isCheck;
				}
			}
		}

		#region Grouping

		// Group selection is done with FastListView

		private void HandleGroupCheckRecursive(CollectionViewGroup group, bool check)
		{
			List<object> items = new List<object>(lvTrace.SelectedItems as IEnumerable<object>);

			HandleGroupCheckRecursive_Internal(group, check, ref items);

			lvTrace.SelectItems(items);
		}

		private void HandleGroupCheckRecursive_Internal(CollectionViewGroup group, bool check, ref List<object> selectedItems)
		{
			foreach (var itemOrGroup in group.Items)
			{
				if (itemOrGroup is CollectionViewGroup)
				{
					// Found a nested group - recursively run this method again
					this.HandleGroupCheckRecursive(itemOrGroup as CollectionViewGroup, check);
				}
				else if (itemOrGroup is TraceLineDesign)
				{
					var item = (TraceLineDesign)itemOrGroup;
					item.IsChecked = check;

					if (check)
						selectedItems.Add(item);
					else
						selectedItems.Remove(item);
				}
			}
		}

		private void OnGroupChecked(object sender, RoutedEventArgs e)
		{
			this.HandleGroupCheck((CheckBox)sender, true);
		}
		private void OnGroupUnchecked(object sender, RoutedEventArgs e)
		{
			this.HandleGroupCheck((CheckBox)sender, false);
		}

		private void HandleGroupCheck(FrameworkElement sender, bool check)
		{
			var group = (CollectionViewGroup)sender.DataContext;
			this.HandleGroupCheckRecursive(group, check);
		}

		private void btnCollapseGroups_Click(object sender, RoutedEventArgs e)
		{
			foreach (var trace in _viewModel.Traces.Where(t => t.IsGroupHeader && t.IsExpanded == true))
			{
				trace.IsExpanded = false;
			}
		}

		private void btnExpandGroups_Click(object sender, RoutedEventArgs e)
		{
			foreach (var trace in _viewModel.Traces.Where(t=> t.IsGroupHeader && t.IsExpanded == false))
			{
				trace.IsExpanded = true;
			}			
		}



		#endregion Grouping

		#region Filtering

		private void btnApplyFilter_Click(object sender, RoutedEventArgs e)
		{
			ApplyFilter(txtFilter.Text);
		}

		private void txtFilter_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				ApplyFilter(txtFilter.Text);
			}
		}

		private void ApplyFilter(string text)
		{
			_viewModel.Filter = text;
		}

		#endregion Filtering
	}
}

