using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit.Viewers
{
	/// <summary>
	/// Spatial viewer custom control GDI+
	/// </summary>
	public partial class SpatialViewer_GDI : Control, ISpatialViewer, IDisposable //, IMessageFilter // for mousewheel
	{
		// geometry bounding box
		BoundingBox _geomBBox;

		bool _readyToDraw = false;

		// Viewport variables
		float _currentFactorMouseWheel = 1f;
		float _scale = 1f;
		float _scaleX = 1f;
		float _scaleY = 1f;
		Matrix _mouseTranslate;
		Matrix _mouseScale;
		Matrix _previousMatrix;
		public bool AutoViewPort { get; set; }

		// GDI+ geometries
		Dictionary<GeometryStyle, List<GraphicsPath>> _strokes;
		Dictionary<GeometryStyle, List<GraphicsPath>> _fills;
		Dictionary<GeometryStyle, List<PointF>> _points;
		Bitmap _pointBmp;

		public SpatialViewer_GDI()
		{
			InitializeComponent();
			//SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			_strokes = new Dictionary<GeometryStyle, List<GraphicsPath>>();
			_fills = new Dictionary<GeometryStyle, List<GraphicsPath>>();
			_points = new Dictionary<GeometryStyle, List<PointF>>();

			_mouseTranslate = new Matrix();
			_mouseScale = new Matrix();
			//System.Windows.Forms.Application.AddMessageFilter(this);
			System.Windows.Forms.Application.AddMessageFilter(new MouseWheelMessageFilter());
			this.MouseWheel += SpatialViewer_GDI_MouseWheel;

			// Load point icon
			Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			using (Stream file = assembly.GetManifestResourceStream("SqlServerSpatial.Toolkit.Viewers.GDI.point.png"))
			{
				_pointBmp = (Bitmap)Image.FromStream(file);
			}
		}

		#region Dispose and Finalize

		~SpatialViewer_GDI()
		{
			Dispose(false);
		}
		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			//clean up unmanaged here

			if (_pointBmp != null)
				_pointBmp.Dispose();

			DisposeGraphicsPaths();
			_mouseTranslate.Dispose();
			_mouseScale.Dispose();
			if (_previousMatrix != null) _previousMatrix.Dispose();
			this.MouseWheel -= SpatialViewer_GDI_MouseWheel;

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void DisposeGraphicsPaths()
		{
			if (_strokes != null)
			{
				foreach (var val in _strokes.Values)
					foreach (var handle in val)
						handle.Dispose();
				_strokes.Clear();
				_strokes = null;
			}
			if (_fills != null)
			{
				foreach (var val in _fills.Values)
					foreach (var handle in val)
						handle.Dispose();
				_fills.Clear();
				_fills = null;
			}
		}

		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion



		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);

			if (_readyToDraw)
			{
				Stopwatch sw = Stopwatch.StartNew();

				using (Matrix mat = GenerateGeometryTransformViewMatrix())
				{
					mat.Multiply(_mouseTranslate, MatrixOrder.Append);
					mat.Multiply(_mouseScale, MatrixOrder.Append);

					if (pe.ClipRectangle != this.ClientRectangle)
					{
						Debug.WriteLine("Partial paint : " + pe.ClipRectangle.ToString());
					}
					pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;

					// Shapes
					foreach (var kvpFill in _fills)
					{
						using (Brush fillBrush = FromGeomStyleToBrush(kvpFill.Key))
						{
							foreach (GraphicsPath path in kvpFill.Value)
							{
								using (GraphicsPath pathClone = (GraphicsPath)path.Clone())
								{
									pathClone.Transform(mat);
									pe.Graphics.FillPath(fillBrush, pathClone);
								}
							}
						}
					}
					// Outlines
					foreach (var kvpStroke in _strokes)
					{
						using (Pen strokePen = FromGeomStyleToPen(kvpStroke.Key))
						{
							foreach (GraphicsPath path in kvpStroke.Value)
							{
								using (GraphicsPath pathClone = (GraphicsPath)path.Clone())
								{
									pathClone.Transform(mat);
									pe.Graphics.DrawPath(strokePen, pathClone);
								}
							}
						}
					}

					// Points
					foreach (var kvpPoint in _points)
					{
						using (Bitmap bmp = FromGeomStyleToPoint(_pointBmp, kvpPoint.Key))
						{
							PointF[] points = kvpPoint.Value.ToArray();
							if (points.Any())
							{
								mat.TransformPoints(points);
								foreach (PointF point in points)
								{
									pe.Graphics.DrawImageUnscaled(bmp, (int)point.X - _pointBmp.Width / 2, (int)point.Y - _pointBmp.Height / 2);
								}
							}
						}
					}
				}

				sw.Stop();
				Debug.WriteLine("{0:g} for draw", sw.Elapsed);

			}
		}

	

		#region GDI Helpers
		private Pen FromGeomStyleToPen(GeometryStyle geometryStyle)
		{
			System.Windows.Media.Color c = geometryStyle.StrokeColor;
			return new Pen(Color.FromArgb(c.A, c.R, c.G, c.B), geometryStyle.StrokeWidth);
		}
		private Brush FromGeomStyleToBrush(GeometryStyle geometryStyle)
		{
			System.Windows.Media.Color c = geometryStyle.FillColor;
			return new SolidBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
		}
		private Bitmap FromGeomStyleToPoint(Bitmap sourceBitmap, GeometryStyle geometryStyle)
		{
			return TintBitmap(sourceBitmap, geometryStyle.FillColor.ToGDI(), 1f);
		}
		private void AppendStrokePath(GeometryStyle style, GraphicsPath stroke)
		{
			if (_strokes.ContainsKey(style) == false)
				_strokes[style] = new List<GraphicsPath>();

			_strokes[style].Add(stroke);
		}
		private void AppendFilledPath(GeometryStyle style, GraphicsPath path)
		{
			if (_fills.ContainsKey(style) == false)
				_fills[style] = new List<GraphicsPath>();

			_fills[style].Add(path);
		}
		private void AppendPoints(GeometryStyle style, List<PointF> points)
		{
			if (_points.ContainsKey(style) == false)
				_points[style] = new List<PointF>();

			_points[style].AddRange(points);
		}
		/// <summary>
		/// Tints a bitmap using the specified color and intensity.
		/// </summary>
		/// <param name="bitmap">Bitmap to be tinted</param>
		/// <param name="color">Color to use for tint</param>
		/// <param name="intensity">Intensity of the tint.  Good ranges are .25 to .75, depending on your preference.  Most images will white out around 2.0. 0 will not tint the image at all</param>
		/// <returns>A bitmap with the requested Tint</returns>
		/// <remarks>http://stackoverflow.com/questions/9356694/tint-property-when-drawing-image-with-vb-net</remarks>
		Bitmap TintBitmap(Bitmap bitmap, Color color, float intensity)
		{
			Bitmap outBmp = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

			using (ImageAttributes ia = new ImageAttributes())
			{

				ColorMatrix m = new ColorMatrix(new float[][] 
        {new float[] {1, 0, 0, 0, 0}, 
         new float[] {0, 1, 0, 0, 0}, 
         new float[] {0, 0, 1, 0, 0}, 
         new float[] {0, 0, 0, 1, 0}, 
         new float[] {color.R/255*intensity, color.G/255*intensity, color.B/255*intensity, 0, 1}});

				ia.SetColorMatrix(m);
				using (Graphics g = Graphics.FromImage(outBmp))
					g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
			}

			return outBmp;
		}

		#endregion

		public void SetGeometry(IEnumerable<SqlGeometryStyled> geometries)
		{
			this.Internal_SetGeometry(geometries);
		}

		Matrix GenerateGeometryTransformViewMatrix()
		{
			if (AutoViewPort == false && _previousMatrix != null)
			{
				return _previousMatrix.Clone();
			}
			else
			{
				int margin = 20;
				float width = this.ClientRectangle.Width - margin;
				float height = this.ClientRectangle.Height - margin;

				Matrix m = new Matrix();

				// Center matrix origin
				m.Translate((float)(-_geomBBox.XMin - _geomBBox.Width / 2d), (float)(-_geomBBox.yMin - _geomBBox.Height / 2d));

				// Scale and invert Y as Y raises downwards
				_scaleX = (float)(width / _geomBBox.Width);
				_scaleY = (float)(height / _geomBBox.Height);
				_scale = (float)Math.Min(_scaleX, _scaleY);
				m.Scale(_scale, -_scale, MatrixOrder.Append);

				// translate to map center
				BoundingBox bboxTrans = _geomBBox.Transform(m);
				m.Translate(width / 2, -(float)bboxTrans.Height / 2f, MatrixOrder.Append);

				if (_previousMatrix != null)
				{
					_previousMatrix.Dispose();
				}
				_previousMatrix = m.Clone();
				return m;
			}
		}

		//void CalculateUnitVector()
		//{
		//	// geom units * scale = pixels
		//	// To get in units what is a pixel, we do W * scale = 1 => W = 1 / scale;
		//	_unitVectorAtGeometryScale = new Vector(1d / (_currentFactorMouseWheel * _scaleX) * 2, 1d / (_currentFactorMouseWheel * _scaleY) * 2);
		//}

		private void Internal_SetGeometry(IEnumerable<SqlGeometryStyled> geometries)
		{
			try
			{
				Stopwatch sw = Stopwatch.StartNew();
				Stopwatch swUnion = new Stopwatch();

				_readyToDraw = false;
				ClearGDI();

				if (geometries == null)
					throw new ArgumentNullException("geometry");

				int srid = 0;
				if (!geometries.Select(b => b.Geometry).AreSridEqual(out srid))
					throw new ArgumentOutOfRangeException("Geometries do not have the same SRID");

				SqlGeometry envelope = SqlTypesExtensions.PointEmpty_SqlGeometry(srid);

				if (geometries.Any(g => g.Geometry.STIsValid().IsFalse))
				{
					System.Windows.Forms.MessageBox.Show("Some geometries are not valid. Will try to valid them.", "Invalid geometry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				foreach (SqlGeometryStyled geomStyled in geometries)
				{

					SqlGeometry geometry = geomStyled.Geometry;
					if (geometry == null || geometry.IsNull)
						throw new ArgumentNullException("geometry");

					if (geometry.STIsValid().IsFalse)
						geometry = geometry.MakeValid();

					// Envelope of Union of envelopes => global BBox
					envelope = envelope.STUnion(geometry.STEnvelope()).STEnvelope();

					GraphicsPath stroke = new GraphicsPath(); GraphicsPath fill = new GraphicsPath();
					List<PointF> points = new List<PointF>();
					SqlGeometryGDISink.ConvertSqlGeometry(geometry, ref stroke, ref fill, ref points);
					AppendFilledPath(geomStyled.Style, fill);
					AppendStrokePath(geomStyled.Style, stroke);
					AppendPoints(geomStyled.Style, points);

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

				Debug.WriteLine("Init : {0} ms", sw.ElapsedMilliseconds);
				Debug.WriteLine("Init other : {0} ms", swUnion.ElapsedMilliseconds);

				_readyToDraw = true;
				Invalidate();
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(this.GetType().Name + " Error: " + ex.Message);
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
						SqlGeometryStyled geomStyled = SqlGeomStyledFactory.Create(geom, geog.Style.Label, geog.Style.FillColor, geog.Style.StrokeColor, geog.Style.StrokeWidth);
						geoms.Add(geomStyled);
					}
				}
				this.Internal_SetGeometry(geoms);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(this.GetType().Name + " Error: " + ex.Message);
			}
		}

		private void ClearGDI()
		{
			DisposeGraphicsPaths();
			_strokes = new Dictionary<GeometryStyle, List<GraphicsPath>>();
			_fills = new Dictionary<GeometryStyle, List<GraphicsPath>>();
			_points = new Dictionary<GeometryStyle, List<PointF>>();

		}
		public void Clear()
		{
			ClearGDI();
			this.Invalidate();
		}

		public void ResetView()
		{
			ResetView(true);
		}
		internal void ResetView(bool fullReset)
		{
			if (fullReset)
			{
				_previousMatrix = null;
				_mouseTranslate = new Matrix();
				_mouseScale = new Matrix();
				_currentFactorMouseWheel = 1f;
			}
			this.Invalidate();
		}

		#region User events

		bool _isMouseDown = false;
		System.Drawing.Point _mouseDownPoint;
		private void SpatialViewer_GDI_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				_isMouseDown = true;
				_mouseDownPoint = e.Location;
			}
		}

		private void SpatialViewer_GDI_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isMouseDown)
			{
				System.Drawing.Point currentMousePos = e.Location;

				//_mouseTranslate.Translate(currentMousePos.X - _mouseDownPoint.X, currentMousePos.Y - _mouseDownPoint.Y);
				_mouseTranslate.Translate((currentMousePos.X - _mouseDownPoint.X) / _currentFactorMouseWheel, (currentMousePos.Y - _mouseDownPoint.Y) / _currentFactorMouseWheel);

				_mouseDownPoint = currentMousePos;

				Invalidate();
			}
		}

		private void SpatialViewer_GDI_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				_isMouseDown = false;
			}
		}

		private void SpatialViewer_GDI_MouseWheel(object sender, MouseEventArgs e)
		{
			float factor = 1.2f;
			if (e.Delta > 0)
			{
				_mouseScale.Scale(factor, factor, MatrixOrder.Append);
				_currentFactorMouseWheel *= factor;
				_mouseScale.Translate(e.X * (1f - factor), e.Y * (1f - factor), MatrixOrder.Append);

			}
			else
			{
				_mouseScale.Scale(1f / factor, 1f / factor, MatrixOrder.Append);
				_currentFactorMouseWheel /= factor;
				_mouseScale.Translate(e.X * (1f - 1f / factor), e.Y * (1f - 1f / factor), MatrixOrder.Append);
			}
			Invalidate();
		}

		#endregion


		public event EventHandler GetSQLSourceText;
	}
}
