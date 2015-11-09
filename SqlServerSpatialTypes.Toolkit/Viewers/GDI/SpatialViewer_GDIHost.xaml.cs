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

namespace SqlServerSpatialTypes.Toolkit.Viewers
{
	/// <summary>
	/// Spatial viewer using GDI+
	/// </summary>
	public partial class SpatialViewer_GDIHost : UserControl, ISpatialViewer
	{
		public SpatialViewer_GDIHost()
		{
			InitializeComponent();
			gdiViewer.AutoViewPort = chkAutoViewPort.IsChecked.Value;
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
	}
}
