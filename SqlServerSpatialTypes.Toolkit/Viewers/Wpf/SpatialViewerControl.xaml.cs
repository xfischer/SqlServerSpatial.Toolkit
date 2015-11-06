using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SqlServerSpatialTypes.Toolkit.Viewers
{
	/// <summary>
	/// Logique d'interaction pour SpatialViewerControl.xaml
	/// </summary>
	public partial class SpatialViewerControl : UserControl, ISpatialViewer
	{
		BoundingBox _geomBBox;
		List<Path> _geomShapeWpf;
		bool _shapeDrawn;
		Rectangle _eventRectangle;

		// Transformation matrix
		Matrix _matrix;
		Vector _unitVectorAtGeometryScale;

		// Pan
		Point _mouseDownPoint;
		bool _isMouseDown;
		private StringBuilder _geomSqlSourceTextBuilder;
		private int _geomSqlSourceCount;

		public SpatialViewerControl()
		{
			InitializeComponent();
		}

		private void ResetSQLSource()
		{
			_geomSqlSourceTextBuilder = null;
			_geomSqlSourceCount = 0;
		}
		private void AppendGeometryToSQLSource(SqlGeometry geom)
		{
			if (_geomSqlSourceTextBuilder == null)
			{
				ResetSQLSource();
				_geomSqlSourceTextBuilder = new StringBuilder();
			}
			else
			{
				_geomSqlSourceTextBuilder.AppendLine();
			}

			_geomSqlSourceTextBuilder.AppendFormat("DECLARE @g{0} geometry = geometry::STGeomFromText('{1}',{2})", ++_geomSqlSourceCount, geom.ToString(), geom.STSrid.Value);
		}
		private void AppendGeometryToSQLSource(SqlGeography geom)
		{
			if (_geomSqlSourceTextBuilder == null)
			{
				ResetSQLSource();
			}
			else
			{
				_geomSqlSourceTextBuilder.AppendLine();
			}

			_geomSqlSourceTextBuilder.AppendFormat("DECLARE @g{0} geography = geography::STGeomFromText('{1}',{2})", ++_geomSqlSourceCount, geom.ToString(), geom.STSrid.Value);
		}

		public void SetGeometry(IEnumerable<SqlGeometryStyled> geometries)
		{
			this.Internal_SetGeometry(geometries, true);
		}
		private void Internal_SetGeometry(IEnumerable<SqlGeometryStyled> geometries, bool appendToSqlSourceText = true)
		{
			try
			{
				Stopwatch sw = Stopwatch.StartNew();

				_geomShapeWpf = new List<Path>();

				if (geometries == null)
					throw new ArgumentNullException("geometry");

				int srid = 0;
				if (!geometries.Select(b => b.Geometry).AreSridEqual(out srid))
					throw new ArgumentOutOfRangeException("Geometries do not have the same SRID");

				SqlGeometry envelope = SqlTypesExtensions.PointEmpty_SqlGeometry(srid);

				// Reset geom sql text
				if (appendToSqlSourceText) ResetSQLSource();

				if (geometries.Any(g=>g.Geometry.STIsValid().IsFalse))
				{
					MessageBox.Show("Some geometries are not valid. Will try to valid them.", "Invalid geometry", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
				foreach (SqlGeometryStyled geomStyled in geometries)
				{
					SqlGeometry geometry = geomStyled.Geometry;
					if (geometry == null || geometry.IsNull)
						throw new ArgumentNullException("geometry");

					if (geometry.STIsValid().IsFalse)
						geometry = geometry.MakeValid();

					// Update geom SQL text (used for "copy" feature)
					if (appendToSqlSourceText) AppendGeometryToSQLSource(geometry);

					_shapeDrawn = false;

					// Envelope of Union of envelopes => global BBox
					envelope = envelope.STUnion(geometry.STEnvelope()).STEnvelope();
				}
				#region BBox
				List<double> xcoords = new List<double>();
				List<double> ycoords = new List<double>();
				for (int i = 1; i <= envelope.STNumPoints(); i++)
				{
					xcoords.Add(envelope.STPointN(i).STX.Value);
					ycoords.Add(envelope.STPointN(i).STY.Value);
				}
				#endregion

				_geomBBox = new BoundingBox(xcoords.Min(), xcoords.Max(), ycoords.Min(), ycoords.Max());
				GenerateGeometryTransformViewMatrix(); // Used to get unit vector


				foreach (SqlGeometryStyled geometry in geometries)
				{
					Path currentShape = geometry.Geometry.ToShapeWpf(new SolidColorBrush(geometry.Style.FillColor), new SolidColorBrush(geometry.Style.StrokeColor), geometry.Style.StrokeWidth, _unitVectorAtGeometryScale);
					_geomShapeWpf.Add(currentShape);
				}


				Trace.TraceInformation("Init : {0} ms", sw.ElapsedMilliseconds);
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
				ResetSQLSource();
				foreach (SqlGeographyStyled geog in geographies)
				{
					// Update geom SQL text (used for "copy" feature)
					AppendGeometryToSQLSource(geog.Geometry);

					SqlGeometry geom = null;
					if (geog.Geometry.TryToGeometry(out geom))
					{
						SqlGeometryStyled geomStyled = SqlGeomStyledFactory.Create(geom, geog.Style.Label, geog.Style.FillColor, geog.Style.StrokeColor, geog.Style.StrokeWidth);
						geoms.Add(geomStyled);
					}
				}
				this.Internal_SetGeometry(geoms, false);
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
			Stopwatch sw = Stopwatch.StartNew();

			if (_shapeDrawn)
			{
				if (chkAutoViewPort.IsChecked.GetValueOrDefault(true))
				{
					_matrix = GenerateGeometryTransformViewMatrix();
					foreach (Path path in _geomShapeWpf)
					{
						path.Data.Transform = new MatrixTransform(_matrix);
					}
				}
				else
				{
					foreach (Path path in _geomShapeWpf)
					{
						path.Data.Transform = new MatrixTransform(_matrix);
					}
				}
			}

			Trace.TraceInformation("ResetView : {0} ms", sw.ElapsedMilliseconds);
		}

		void Draw()
		{
			Stopwatch sw = Stopwatch.StartNew();

			if (_geomShapeWpf == null)
				return;

			if (_shapeDrawn)
				return;

			map.Children.Clear();

			// captures mouse events even if mouse not on shape
			_eventRectangle = new Rectangle() { Width = map.ActualWidth, Height = map.ActualHeight, Fill = new SolidColorBrush(Color.FromRgb(250, 250, 250)) };

			map.Children.Add(_eventRectangle);
			foreach (Path geom in _geomShapeWpf)
			{
				map.Children.Add(geom);
			}


			_shapeDrawn = true;

			Trace.TraceInformation("Draw : {0} ms", sw.ElapsedMilliseconds);


			ResetView();


		}

		public WriteableBitmap SaveAsWriteableBitmap(Canvas surface)
		{
			if (surface == null) return null;

			// Save current canvas transform
			Transform transform = surface.LayoutTransform;
			// reset current transform (in case it is scaled or rotated)
			surface.LayoutTransform = null;

			// Get the size of canvas
			Size size = new Size(surface.ActualWidth, surface.ActualHeight);
			// Measure and arrange the surface
			// VERY IMPORTANT
			surface.Measure(size);
			surface.Arrange(new Rect(size));

			// Create a render bitmap and push the surface to it
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
				(int)size.Width,
				(int)size.Height,
				96d,
				96d,
				PixelFormats.Pbgra32);
			renderBitmap.Render(surface);


			//Restore previously saved layout
			surface.LayoutTransform = transform;

			//create and return a new WriteableBitmap using the RenderTargetBitmap
			return new WriteableBitmap(renderBitmap);

		}

		#region Helpers

		Matrix GenerateGeometryTransformViewMatrix()
		{
			Matrix m = Matrix.Identity;

			if (_shapeDrawn && chkAutoViewPort.IsChecked.Value == false)
			{
				m = _matrix;
			}
			else
			{
				double width = map.ActualWidth, height = map.ActualHeight;

				m = Matrix.Identity;

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
			}

			CalculateUnitVector(m);

			return m;
		}

		void SetGeometryTransform(Matrix matrix)
		{
			foreach (Path path in _geomShapeWpf)
			{
				((MatrixTransform)path.Data.Transform).Matrix = matrix;
			}
			foreach (Path path in _geomShapeWpf)
			{
				((MatrixTransform)path.Data.Transform).Matrix = matrix;
			}
			CalculateUnitVector(matrix);
		}
		void CalculateUnitVector(Matrix matrix)
		{
			if (matrix.HasInverse)
			{
				matrix.Invert();
				double width = map.ActualWidth, height = map.ActualHeight;
				double scale = Math.Min(width / _geomBBox.Width
															, height / _geomBBox.Height);
				Vector vector1px = new Vector(1, 0) / scale;
				_unitVectorAtGeometryScale = vector1px; //matrix.Transform(vector1px);
			}
			else
			{
				_unitVectorAtGeometryScale = new Vector(1, 1);
			}
		}

		#endregion Helpers

		#region UI events

		// Window resized
		void map_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_shapeDrawn)
			{
				_eventRectangle.Width = map.ActualWidth;
				_eventRectangle.Height = map.ActualHeight;
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
			bool capture = map.CaptureMouse();
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
			map.ReleaseMouseCapture();
		}

		#endregion UI events

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			ResetView();
		}

		private void btnCopy_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(_geomSqlSourceTextBuilder.ToString());
		}



	}
}
