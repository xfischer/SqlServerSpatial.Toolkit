using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SqlServerSpatial.Toolkit.BaseLayer;
using System.IO;
using GDIImage = System.Drawing.Image;
using System.Drawing;
using Microsoft.Win32;
using System.ComponentModel;

namespace SqlServerSpatial.Toolkit.Viewers
{
	/// <summary>
	/// Logique d'interaction pour SpatialViewer_GDIHost.xaml
	/// </summary>
	public partial class SpatialViewer_GDIHost : UserControl, ISpatialViewer
	{

		IMapViewModel _viewModel;
	
		public SpatialViewer_GDIHost()
		{
			InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
			{
				_viewModel = new MapViewModel(null);
			}
			else
			{
				_viewModel = new MapViewModel(gdiViewer);
			}
			this.DataContext = _viewModel;

			gdiViewer.AutoViewPort = chkAutoViewPort.IsChecked.Value;
			gdiViewer.InfoMessageSent += gdiViewer_InfoMessageSent;			
			
		}

		void gdiViewer_InfoMessageSent(object sender, GDI.ViewerInfoEventArgs e)
		{
			if (e.InfoType.HasFlag(GDI.ViewerInfoType.InitDone))
			{
				GeomInfoLabel.Text = e.GeometryInfo;
				PerfInitLabel.Text = string.Format("Init: {0} ms", e.InitTime);
				MouseCoordsLabel.Text = null;
			}
			else if (e.InfoType.HasFlag(GDI.ViewerInfoType.MouseMove))
			{
				MouseCoordsLabel.Text = "Mouse move";
			}
			else if (e.InfoType.HasFlag(GDI.ViewerInfoType.Draw))
			{
				PerfDrawLabel.Text = string.Format("Draw: {0} ms", e.DrawTime);
			}

		}

		#region ISpatialViewer Membres

		public void SetGeometry(SqlGeometryStyled geometry)
		{
			gdiViewer.SetGeometry(geometry);
		}

		public void SetGeometry(IEnumerable<SqlGeometryStyled> geometries)
		{
			gdiViewer.SetGeometry(geometries);
		}

		public void SetGeometry(SqlGeographyStyled geography)
		{
			gdiViewer.SetGeometry(geography);
		}

		public void SetGeometry(IEnumerable<SqlGeographyStyled> geographies)
		{
			gdiViewer.SetGeometry(geographies);
		}

		public void Clear()
		{
			gdiViewer.Clear();
		}

		public void ResetView()
		{
			if (chkAutoViewPort.IsChecked.GetValueOrDefault(true))
			{
				gdiViewer.ResetView(true);
			}
			else
			{
				gdiViewer.ResetView(false);
			}
		}

		#endregion

		#region User Events

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			ResetView();
		}

		private void btnCopy_Click(object sender, RoutedEventArgs e)
		{
			string data = gdiViewer.GetSQLSourceText();
			if (data != null) Clipboard.SetText(data);
		}

		private void chkAutoViewPort_Click(object sender, RoutedEventArgs e)
		{
			gdiViewer.AutoViewPort = chkAutoViewPort.IsChecked.GetValueOrDefault(true);
		}
		
		private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			IBaseLayerViewer baseLayerViewer = gdiViewer as IBaseLayerViewer;
			if (baseLayerViewer != null)
			{
				baseLayerViewer.Opacity = (float)(e.NewValue / 100d);
			}
		}

		private void chkLabels_Click(object sender, RoutedEventArgs e)
		{
			IBaseLayerViewer baseLayerViewer = gdiViewer as IBaseLayerViewer;
			if (baseLayerViewer != null)
			{
				baseLayerViewer.ShowLabels = chkLabels.IsChecked.Value;
			}
		}

		private void btnExport_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "PNG Image file (*.png)|*.png";
			dlg.Title = "Export image";
			dlg.ValidateNames = true;
			if (dlg.ShowDialog() == true)
			{
				using (Bitmap bmp = new Bitmap(gdiViewer.Width, gdiViewer.Height))
				{
					gdiViewer.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
					bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
				}
			}

		}

		#endregion User Events


		string ISpatialViewer.GetSQLSourceText()
		{
			return gdiViewer.GetSQLSourceText();
		}
	}
}
