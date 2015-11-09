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
using Microsoft.SqlServer.Types;

namespace SqlServerSpatialTypes.Toolkit.Viewers
{
	/// <summary>
	/// Logique d'interaction pour SpatialViewer_DrawingContext.xaml
	/// </summary>
	public partial class SpatialViewer_DrawingContext : UserControl, ISpatialViewer
	{
		BoundingBox _geomBBox;
		List<Path> _geomShapeWpf;
		bool _shapeDrawn;
		VisualHostContainer _dcVisual;

		// Transformation matrix
		Matrix _matrix;

		// Pan
		Point _mouseDownPoint;
		bool _isMouseDown;

		public SpatialViewer_DrawingContext()
		{
			InitializeComponent();
		}

		public void SetGeometry(IEnumerable<SqlGeometryStyled> geometries)
		{
			try
			{
				_geomShapeWpf = new List<Path>();

				if (geometries == null)
					throw new ArgumentNullException("geometry");

				int srid = 0;
				if (!geometries.Select(b => b.Geometry).AreSridEqual(out srid))
					throw new ArgumentOutOfRangeException("Geometries do not have the same SRID");

				SqlGeometry envelope = SqlTypesExtensions.PointEmpty_SqlGeometry(srid);

				foreach (SqlGeometryStyled geomStyled in geometries)
				{
					SqlGeometry geometry = geomStyled.Geometry;
					if (geometry == null || geometry.IsNull)
						throw new ArgumentNullException("geometry");

					if (geometry.STIsValid().IsFalse)
						throw new ArgumentException("geometry is invalid. Please use MakeValid()", "geometry");

					_shapeDrawn = false;

					// Envelope of Union of envelopes => global BBox
					envelope = envelope.STUnion(geometry.STEnvelope()).STEnvelope();

					Path currentShape = geometry.ToShapeWpf(new SolidColorBrush(Color.FromRgb(0, 175, 0)), new SolidColorBrush(Colors.Black), 1, new Vector(1, 1));
					_geomShapeWpf.Add(currentShape);
				}

				#region BBox
				List<double> xcoords = new List<double>();
				List<double> ycoords = new List<double>();
				for (int i = 1; i <= envelope.STNumPoints(); i++)
				{
					xcoords.Add(envelope.STPointN(i).STX.Value);
					ycoords.Add(envelope.STPointN(i).STY.Value);
				}
				_geomBBox = new BoundingBox(xcoords.Min(), xcoords.Max(), ycoords.Min(), ycoords.Max());
				#endregion

				//this.Viewer.InitView();
				Draw();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this.GetType().Name + " Error: " + ex.Message);
			}
		}
		public void SetGeometry(SqlGeometryStyled geometry)
		{
			SetGeometry(new SqlGeometryStyled[] { geometry });
		}
		public void SetGeometry(SqlGeographyStyled geography)
		{
			SetGeometry(new SqlGeographyStyled[] { geography });
		}
		public void SetGeometry(IEnumerable<SqlGeographyStyled> geographies)
		{
			try
			{
				List<SqlGeometryStyled> geoms = new List<SqlGeometryStyled>();
				foreach (SqlGeographyStyled geog in geographies)
				{
					SqlGeometry geom = null;
					if (geog.Geometry.TryToGeometry(out geom))
					{
						SqlGeometryStyled geomStyled = SqlGeomStyledFactory.Create(geom,geog.Style.Label, geog.Style.FillColor, geog.Style.StrokeColor, geog.Style.StrokeWidth);
						geoms.Add(geomStyled);
					}
				}
				this.SetGeometry(geoms);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this.GetType().Name + " Error: " + ex.Message);
			}
		}
		public void Clear()
		{
			map.Children.Clear();
		}


		public void ResetView()
		{
			if (_shapeDrawn)
			{
				_matrix = GenerateGeometryTransformViewMatrix();
				//_dcVisual.LayoutTransform = new MatrixTransform(_matrix);
				foreach (Path path in _geomShapeWpf)
				{
					path.Data.Transform = new MatrixTransform(_matrix);
				}
			}
		}

		void Draw()
		{
			if (_geomShapeWpf == null)
				return;

			if (_shapeDrawn)
				return;

			map.Children.Clear();



			// captures mouse events even if mouse not on shape
			//_eventRectangle = new Rectangle() { Width = map.ActualWidth, Height = map.ActualHeight, Fill = new SolidColorBrush(Color.FromArgb (25,250, 250, 250)) };
			_dcVisual = new VisualHostContainer();
			_dcVisual.ClipToBounds = true;
			_dcVisual.MouseWheel += map_MouseWheel;
			_dcVisual.MouseLeftButtonDown += map_MouseLeftButtonDown;
			_dcVisual.MouseMove += map_MouseMove;
			_dcVisual.MouseLeftButtonUp += map_MouseLeftButtonUp;
			//map.Children.Add(_eventRectangle);
			//foreach (Path geom in _geomShapeWpf)
			//{
			//	map.Children.Add(geom);
			//}


			foreach (Path geom in _geomShapeWpf)
			{
				_dcVisual.AddGeometry(geom.Data);
			}
			map.Children.Add(_dcVisual);
			//map.Children.Add(_eventRectangle);
			_shapeDrawn = true;
			ResetView();
		}

		#region Helpers

		Matrix GenerateGeometryTransformViewMatrix()
		{
			double width = map.ActualWidth, height = map.ActualHeight;

			Matrix m = Matrix.Identity;

			double scale = Math.Min(width / _geomBBox.Width
															, height / _geomBBox.Height);

			double translateX = -_geomBBox.XMin * scale																		// top left bbox corner at origin
													- (_geomBBox.Width * 0.5d * scale)												// center on bbox middle
													+ width / 2d;																		// centered on canvas
			//+ _viewTranslateX;																				// mouse drag displacement
			double translateY = -_geomBBox.yMin * scale - (_geomBBox.Height / 2d * scale) + height / 2d;// +_viewTranslateY;

			//  geom must fit in window
			m.Scale(scale, scale);

			//  translate bbox at window center
			m.Translate(translateX, translateY);

			// Flip horizontally, as Y screen coordinates are downwards
			m.ScaleAt(1, -1, width / 2d, height / 2d);

			return m;
		}

		void SetGeometryTransform(Matrix matrix)
		{
			foreach (Path path in _geomShapeWpf)
			{
				((MatrixTransform)path.Data.Transform).Matrix = matrix;
			}
		}

		#endregion Helpers

		#region UI events

		// Window resized
		void map_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_shapeDrawn)
			{
				_dcVisual.Width = map.ActualWidth;
				_dcVisual.Height = map.ActualHeight;
			}
		}

		// Zoom
		void map_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				_matrix.ScaleAt(
							1.2,
							1.2,
							e.GetPosition(map).X,
							e.GetPosition(map).Y);

			}
			else
			{
				_matrix.ScaleAt(
							1.0 / 1.2,
							1.0 / 1.2,
							e.GetPosition(map).X,
							e.GetPosition(map).Y);
			}

			SetGeometryTransform(_matrix);
		}


		// Pan start
		void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			bool capture = _dcVisual.CaptureMouse();
			_mouseDownPoint = e.GetPosition(map);
			_isMouseDown = true;
		}

		// Pan in progress
		void map_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && _isMouseDown)
			{
				Point currentMousePos = e.GetPosition(map);

				_matrix.Translate(currentMousePos.X - _mouseDownPoint.X, currentMousePos.Y - _mouseDownPoint.Y);

				SetGeometryTransform(_matrix);

				_mouseDownPoint = currentMousePos;
			}
		}

		// Pan ends
		void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			_dcVisual.ReleaseMouseCapture();
		}

		#endregion UI events

	}
}
