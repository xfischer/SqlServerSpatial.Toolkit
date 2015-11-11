using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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
		BoundingBox _geomBBox;
		Dictionary<GeometryStyle, List<GraphicsPath>> _strokes;
		Dictionary<GeometryStyle, List<GraphicsPath>> _fills;
		Matrix _mouseTranslate;
		Matrix _mouseScale;
		Matrix _previousMatrix;
		Vector _unitVectorAtGeometryScale;
		bool _readyToDraw = false;
		public bool AutoViewPort { get; set; }

		float _currentFactorMouseWheel = 1f;

		public SpatialViewer_GDI()
		{
			InitializeComponent();
			//SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			_strokes = new Dictionary<GeometryStyle, List<GraphicsPath>>();
			_fills = new Dictionary<GeometryStyle, List<GraphicsPath>>();

			_mouseTranslate = new Matrix();
			_mouseScale = new Matrix();
			//System.Windows.Forms.Application.AddMessageFilter(this);
			System.Windows.Forms.Application.AddMessageFilter(new MouseWheelMessageFilter());
			this.MouseWheel += SpatialViewer_GDI_MouseWheel;
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
				float width = this.ClientRectangle.Width;
				float height = this.ClientRectangle.Height;

				Matrix m = new Matrix();

				// Center matrix origin
				m.Translate((float)(-_geomBBox.XMin - _geomBBox.Width / 2d), (float)(-_geomBBox.yMin - _geomBBox.Height / 2d));

				// Scale and invert Y as Y raises downwards
				double scale = Math.Min(width / _geomBBox.Width, height / _geomBBox.Height);
				m.Scale((float)scale, -(float)scale, MatrixOrder.Append);

				// translate to map center
				BoundingBox bboxTrans = _geomBBox.Transform(m);
				m.Translate(width / 2, -(float)bboxTrans.Height / 2f, MatrixOrder.Append);

				if (_previousMatrix != null)
				{
					_previousMatrix.Dispose();
				}
				_previousMatrix = m.Clone();
				CalculateUnitVector(m, width, height);
				return m;
			}
		}

		void CalculateUnitVector(Matrix mat, float mapWidth, float mapHeight)
		{
			using (Matrix matrix = mat.Clone())
			{
				if (matrix.IsInvertible)
				{
					matrix.Invert();
					double width = mapWidth, height = mapHeight;
					double scale = Math.Min(width / _geomBBox.Width
																, height / _geomBBox.Height);
					PointF vector1px = new PointF((float)(1d / scale), 0);
					_unitVectorAtGeometryScale = new Vector(vector1px.X, vector1px.Y);
				}
				else
				{
					_unitVectorAtGeometryScale = new Vector(1, 1);
				}
			}
		}

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
					SqlGeometryGDISink.ConvertSqlGeometry(geometry, _unitVectorAtGeometryScale, ref stroke, ref fill);
					AppendFilledPath(geomStyled.Style, fill);
					AppendStrokePath(geomStyled.Style, stroke);

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
