using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SqlServerSpatial.Toolkit.BaseLayer;
using SqlServerSpatial.Toolkit.Viewers.GDI;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SqlServerSpatial.Toolkit.Viewers
{
    /// <summary>
    /// Spatial viewer custom control GDI+
    /// </summary>
    public partial class SpatialViewer_GDI : Control, ISpatialViewer, IBaseLayerViewer, IDisposable //, IMessageFilter // for mousewheel
    {
        #region Private variables

        // geometry bounding box
        BoundingBox _geomBBox;
        BoundingBox _geomBBoxNotDefault;

        bool _readyToDraw = false;

        // Viewport variables
        float _currentFactorMouseWheel = 1f;
        float _scale = 1f;
        float _scaleX = 1f;
        float _scaleY = 1f;
        Matrix _mouseTranslate;
        Matrix _mouseScale;
        Matrix _previousMatrix;

        private bool _autoViewPort = false;
        public bool AutoViewPort
        {
            get
            {
                return _autoViewPort;
            }
            set
            {
                _autoViewPort = value;
            }
        }

        // GDI+ geometries
        Dictionary<GeometryStyle, List<GraphicsPath>> _strokes;
        Dictionary<GeometryStyle, List<GraphicsPath>> _fills;
        Dictionary<GeometryStyle, List<PointF>> _points;
        Dictionary<IGeometry, string> _labels;
        Bitmap _pointBmp;

        #endregion

        public SpatialViewer_GDI()
        {
            InitializeComponent();

            try
            {
                //SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);

                _strokes = new Dictionary<GeometryStyle, List<GraphicsPath>>();
                _fills = new Dictionary<GeometryStyle, List<GraphicsPath>>();
                _points = new Dictionary<GeometryStyle, List<PointF>>();
                _labels = new Dictionary<IGeometry, string>();

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

                _tileDownloader = new TileDownloader();
            }
            catch (Exception)
            {


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

            if (_tileDownloader != null)
                _tileDownloader.Dispose();

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

            if (!_readyToDraw)
                return;

            Stopwatch sw = Stopwatch.StartNew();

            using (Matrix mat = GenerateGeometryTransformViewMatrix())
            {
                mat.Multiply(_mouseTranslate, MatrixOrder.Append);
                mat.Multiply(_mouseScale, MatrixOrder.Append);

                if (pe.ClipRectangle != this.ClientRectangle)
                {
                    Trace.TraceInformation("Partial paint : " + pe.ClipRectangle.ToString());
                }


                if (_IBaseLayerViewer.Enabled && _invalidateBackground)
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    Stopwatch swBase = Stopwatch.StartNew();
                    DrawBaseLayer(mat, pe.Graphics);
                    Trace.TraceInformation("{0:g} for base layer", swBase.Elapsed);
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
                                try
                                {
                                    pe.Graphics.FillPath(fillBrush, pathClone);
                                }
                                catch (OverflowException)
                                {
                                    Trace.TraceWarning("DrawShapes: overflow exception");
                                    break;
                                }

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
                                try
                                {
                                    pe.Graphics.DrawPath(strokePen, pathClone);
                                }
                                catch (OverflowException)
                                {
                                    Trace.TraceWarning("DrawOutlines: overflow exception");
                                    break;
                                }

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

                // Labels
                if (_showLabels)
                {
                    // local cache to avoid overlapping labels
                    List<RectangleF> labelRects = new List<RectangleF>();
                    using (StringFormat format = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        foreach (var kvplabel in _labels)
                        {
                            string label = kvplabel.Value;

                            PointF[] centroidasArray = new PointF[] { new PointF((float)kvplabel.Key.Coordinate.X, (float)kvplabel.Key.Coordinate.Y) };
                            mat.TransformPoints(centroidasArray);
                            SizeF labelSize = pe.Graphics.MeasureString(label, SystemFonts.SmallCaptionFont, centroidasArray[0], format);
                            RectangleF rect = new RectangleF(centroidasArray[0], labelSize);

                            if (labelRects.Any(r => r.IntersectsWith(rect)))
                            {
                                //Trace.TraceInformation("RECT Label {0} ignored.", label);
                            }
                            else
                            {
                                pe.Graphics.DrawString(label, SystemFonts.SmallCaptionFont, Brushes.Black, centroidasArray[0], format);
                                labelRects.Add(rect);
                            }
                        }
                    }
                }

            }

            sw.Stop();
            Trace.TraceInformation("{0:g} for draw", sw.Elapsed);
            Raise_InfoMessageSent_Draw(sw.ElapsedMilliseconds);
        }

        #region GDI Helpers
        private Pen FromGeomStyleToPen(GeometryStyle geometryStyle)
        {
            System.Windows.Media.Color c = geometryStyle.StrokeColor;
            Pen v_pen = new Pen(Color.FromArgb(c.A, c.R, c.G, c.B), geometryStyle.StrokeWidth);
            v_pen.LineJoin = LineJoin.Round;
            return v_pen;
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

        private void AppendLabel(IGeometry geometry, string label)
        {
            if (geometry == null || geometry.IsEmpty || string.IsNullOrWhiteSpace(label))
                return;

            try
            {
                IPoint centroid = geometry.Centroid;
                if (centroid == null || centroid.IsEmpty)
                {
                    centroid = geometry.Envelope.Centroid;
                }

                if (centroid != null && centroid.IsEmpty == false)
                    _labels[centroid] = label;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Error in AppendLabel : " + ex.Message);
            }
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
                 new float[] {color.R/255f*intensity, color.G/255f*intensity, color.B/255f*intensity, 0, 1}});

                ia.SetColorMatrix(m);
                using (Graphics g = Graphics.FromImage(outBmp))
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
            }

            return outBmp;
        }

        #endregion

        #region Clipboard (CopySQL feature)

        private SpatialClipboard _clipboard = new SpatialClipboard();
        public string GetSQLSourceText()
        {
            return _clipboard.GetSQLSourceText();
        }

        #endregion

        #region Core

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

                BoundingBox bbox = _geomBBoxNotDefault.IsEmpty ? _geomBBox : _geomBBoxNotDefault;

                // Center matrix origin
                m.Translate((float)(-bbox.XMin - bbox.Width / 2d), (float)(-bbox.YMin - bbox.Height / 2d));

                // Scale and invert Y as Y raises downwards
                _scaleX = (float)(width / bbox.Width);
                _scaleY = (float)(height / bbox.Height);
                _scale = (float)Math.Min(_scaleX, _scaleY);
                m.Scale(_scale, -_scale, MatrixOrder.Append);

                // translate to map center
                BoundingBox bboxTrans = bbox.Transform(m);
                m.Translate(width / 2, -(float)bboxTrans.Height / 2f, MatrixOrder.Append);

                if (_previousMatrix != null)
                {
                    _previousMatrix.Dispose();
                }
                _previousMatrix = m.Clone();
                return m;
            }
        }

        IEnumerable<IGeometryStyled> _geometriesCache;
        private void Internal_SetGeometry(IEnumerable<IGeometryStyled> geometries)
        {
            try
            {
                if (geometries == null)
                    return;

                geometries = geometries.Where(g => g != null && g.Geometry != null && g.Geometry.IsEmpty == false);

                _clipboard.ClipboardGeometries = geometries.Select(g => g.Geometry).ToList();
                _geometriesCache = geometries;
                Stopwatch sw = Stopwatch.StartNew();
                Stopwatch swOther = new Stopwatch();

                _readyToDraw = false;
                ClearGDI();

                if (geometries.Any() == false)
                {
                    InvalidateWrapper(true, true);
                    return;
                }

                if (geometries == null)
                    throw new ArgumentNullException("geometry");

                int srid = 0;
                if (!geometries.Select(b => b.Geometry).AreSridEqual(out srid))
                    throw new ArgumentOutOfRangeException("Geometries do not have the same SRID");

                // Reprojection config
                // Reproject only if base layer SRID is enabled and differs from geometry SRID
                bool bReproject = false;
                int destSrid = srid;
                if (_IBaseLayerViewer.Enabled || bReproject)
                {
                    bReproject = true;
                    destSrid = 4326;
                }

                IGeometry envelope = SqlTypesExtensions.PointEmpty_IGeometry(destSrid);
                IGeometry envelopeNotInDefaultView = SqlTypesExtensions.PointEmpty_IGeometry(destSrid);

                Stopwatch swReproj = new Stopwatch();

                bool hasGeomTaggedAsInDefaultView = false;
                foreach (IGeometryStyled geomStyled in geometries)
                {
                    if (geomStyled == null || geomStyled.Geometry == null || geomStyled.Geometry.IsEmpty)
                    {
                        continue;
                    }

                    try
                    {
                        IGeometry geometry = geomStyled.Geometry;
                        
                        if (geomStyled.Style.IsInDefaultView)
                        {
                            hasGeomTaggedAsInDefaultView = true;
                        }

                        if (bReproject)
                        {
                            swReproj.Start();
                            // Reproj vers mercator
                            geometry = geometry.ReprojectTo(destSrid);
                            // Reproj vers ecran							
                            geometry = IGeometryReprojection.ReprojectToMercator(geometry);
                            geometry.SRID = destSrid;
                            swReproj.Stop();
                        }

                        // Envelope of Union of envelopes => global BBox
                        if (geomStyled.Geometry.Dimension == Dimension.Point)
                        {
                            envelope = envelope.Union(geometry.Buffer(bReproject ? 0.00001 : 1).Envelope).Envelope;
                        }
                        else
                        {
                            envelope = envelope.Union(geometry.Envelope).Envelope;
                        }

                        if (!geomStyled.Style.IsInDefaultView)
                        {
                            if (geomStyled.Geometry.Dimension == Dimension.Point)
                            {
                                // buffer the point
                                envelopeNotInDefaultView = envelopeNotInDefaultView.Union(geometry.Buffer(bReproject ? 0.00001 : 1).Envelope).Envelope;
                            }
                            else
                            {
                                envelopeNotInDefaultView = envelopeNotInDefaultView.Union(geometry.Envelope).Envelope;
                            }
                        }

                        GraphicsPath stroke = new GraphicsPath(); GraphicsPath fill = new GraphicsPath();
                        List<PointF> points = new List<PointF>();
                        IGeometryGDISink.ConvertIGeometry(geometry, ref stroke, ref fill, ref points);
                        AppendFilledPath(geomStyled.Style, fill);
                        AppendStrokePath(geomStyled.Style, stroke);
                        AppendPoints(geomStyled.Style, points);
                        if (_showLabels)
                        {
                            AppendLabel(geometry, geomStyled.Label);
                        }
                    }
                    catch (Exception exGeom)
                    {
                        swReproj.Stop();
                        Trace.TraceError(exGeom.Message);
                    }
                }

                #region BBox
                // ------------------------------------------
                // BBox
                //
                List<double> xcoords = new List<double>();
                List<double> ycoords = new List<double>();
                for (int i = 0; i < envelope.NumPoints; i++)
                {
                    xcoords.Add(envelope.Coordinates[i].X);
                    ycoords.Add(envelope.Coordinates[i].Y);
                }

                _geomBBox = new BoundingBox(xcoords.Min(), xcoords.Max(), ycoords.Min(), ycoords.Max());

                //
                // BBox default view
                //
                if (hasGeomTaggedAsInDefaultView && envelopeNotInDefaultView.Points().Any())
                {
                    xcoords.Clear();
                    ycoords.Clear();
                    for (int i = 0; i < envelopeNotInDefaultView.NumPoints; i++)
                    {
                        xcoords.Add(envelopeNotInDefaultView.Coordinates[i].X);
                        ycoords.Add(envelopeNotInDefaultView.Coordinates[i].Y);
                    }

                    _geomBBoxNotDefault = new BoundingBox(xcoords.Min(), xcoords.Max(), ycoords.Min(), ycoords.Max());
                }
                else
                {
                    _geomBBoxNotDefault = new BoundingBox();
                }
                //
                // ------------------------------------------
                #endregion BBox

                swOther.Start();
                string v_geomInfo = GetGeometryInfo(geometries.Select(g => g.Geometry).ToList());
                swOther.Stop();

                Trace.TraceInformation("Reprojection: {0} ms", swReproj.ElapsedMilliseconds);
                Trace.TraceInformation("Init: {0} ms", sw.ElapsedMilliseconds);
                Trace.TraceInformation("Init other: {0} ms", swOther.ElapsedMilliseconds);


                Raise_InfoMessageSent_Init(v_geomInfo, sw.ElapsedMilliseconds);

                _readyToDraw = true;
                InvalidateWrapper(true, true);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(this.GetType().Name + " Error: " + ex.Message);
            }
        }


        // Get geometry informational message
        private string GetGeometryInfo(List<IGeometry> geometries)
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendFormat("{0} {1}", geometries.Count , geometries.Count == 1 ? " geometry " : "geometries");

            int numParts = geometries.Sum(g => g.NumGeometries);
            int numPoints = geometries.Sum(g => g.NumPoints);
            var reportCountByType = geometries.SelectMany(g => g.Geometries()).GroupBy(g => g.OgcGeometryType).Select(g => new { Type = g.Key, Count = g.Count() }).ToList();
            for (int i = 0; i < reportCountByType.Count; i++)
            {
                var reportItem = reportCountByType[i];
                sb.AppendFormat("{0} {1}", reportItem.Count, reportItem.Type);
                if (i < reportCountByType.Count - 1)
                    sb.Append(", ");
            }


            return sb.ToString();
        }
        public void SetGeometry(IEnumerable<IGeometryStyled> geometries)
        {
            this.Internal_SetGeometry(geometries);
        }
        public void SetGeometry(IGeometryStyled geometry)
        {
            SetGeometry(new IGeometryStyled[] { geometry });
        }
        

        private void ClearGDI()
        {
            DisposeGraphicsPaths();
            _strokes = new Dictionary<GeometryStyle, List<GraphicsPath>>();
            _fills = new Dictionary<GeometryStyle, List<GraphicsPath>>();
            _points = new Dictionary<GeometryStyle, List<PointF>>();
            _labels = new Dictionary<IGeometry, string>();

        }
        public void Clear()
        {
            ClearGDI();
            this.InvalidateWrapper();
        }

        bool _invalidateBackground = true;
        bool _invalidateGeometry = true;
        private void InvalidateWrapper(bool invalidateBackground = true, bool invalidateGeometry = true)
        {
            _invalidateBackground = invalidateBackground;
            _invalidateGeometry = invalidateGeometry;
            this.Invalidate();
        }

        public void ResetView()
        {
            ResetView(true);
        }
        internal void ResetView(bool fullReset)
        {
            if (fullReset || true)
            {
                _previousMatrix = null;
                _mouseTranslate = new Matrix();
                _mouseScale = new Matrix();
                _currentFactorMouseWheel = 1f;
            }
            this.Refresh();
        }

        #endregion

        #region Events

        bool _isMouseDown = false;
        System.Drawing.Point _mouseDownPoint;
        System.Drawing.Point _lastmouseDownPoint;
        private void SpatialViewer_GDI_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _isMouseDown = true;
                Debug.WriteLine("MouseDown: {0}/{1} {2}/{3}", e.X, e.Y, e.Location.X, e.Location.Y);
                _mouseDownPoint = _lastmouseDownPoint = e.Location;
            }
        }

        private void SpatialViewer_GDI_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                Debug.WriteLine("MouseMove: {0}/{1} {2}/{3}", e.X, e.Y, e.Location.X, e.Location.Y);
                System.Drawing.Point currentMousePos = e.Location;

                //_mouseTranslate.Translate(currentMousePos.X - _mouseDownPoint.X, currentMousePos.Y - _mouseDownPoint.Y);
                _mouseTranslate.Translate((currentMousePos.X - _lastmouseDownPoint.X) / _currentFactorMouseWheel, (currentMousePos.Y - _lastmouseDownPoint.Y) / _currentFactorMouseWheel);

                _lastmouseDownPoint = currentMousePos;

                InvalidateWrapper(false, true);
            }
        }

        private void SpatialViewer_GDI_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Debug.WriteLine("MouseUp: {0}/{1} {2}/{3}", e.X, e.Y, e.Location.X, e.Location.Y);
                _isMouseDown = false;
                if (!_mouseDownPoint.Equals(e.Location))
                {
                    this.InvalidateWrapper(true, true);
                }
            }
        }

        private void SpatialViewer_GDI_MouseWheel(object sender, MouseEventArgs e)
        {
            Zoom(e.Delta, e.X, e.Y);
        }

        public void Zoom(int delta, int x, int y)
        {
            if (delta == 0)
                return;

            float factor = 1.2f;
            if (delta > 0)
            {
                _mouseScale.Scale(factor, factor, MatrixOrder.Append);
                _currentFactorMouseWheel *= factor;
                _mouseScale.Translate(x * (1f - factor), y * (1f - factor), MatrixOrder.Append);

            }
            else
            {
                _mouseScale.Scale(1f / factor, 1f / factor, MatrixOrder.Append);
                _currentFactorMouseWheel /= factor;
                _mouseScale.Translate(x * (1f - 1f / factor), y * (1f - 1f / factor), MatrixOrder.Append);
            }
            InvalidateWrapper(true, true);
        }


        public event EventHandler<ViewerInfoEventArgs> InfoMessageSent;
        void Raise_InfoMessageSent_Init(string geomInfo, long initTimeMs)
        {
            if (InfoMessageSent != null)
            {
                try
                {
                    var args = new ViewerInfoEventArgs() { InfoType = ViewerInfoType.InitDone, GeometryInfo = geomInfo, InitTime = initTimeMs };
                    InfoMessageSent(this, args);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Raise_InfoMessageSent_Init : " + ex.Message);
                }
            }
        }
        void Raise_InfoMessageSent_Draw(long drawTimeMs)
        {
            if (InfoMessageSent != null)
            {
                try
                {
                    var args = new ViewerInfoEventArgs() { InfoType = ViewerInfoType.Draw, DrawTime = drawTimeMs };
                    InfoMessageSent(this, args);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Raise_InfoMessageSent_Draw : " + ex.Message);
                }
            }
        }


        #endregion

        #region IBaseLayerViewer

        TileDownloader _tileDownloader;
        IBaseLayer _baseLayer;
        bool _isBaseLayerEnabled;
        bool _showLabels = false;
        private float _opacity = 1f;
        HashSet<GeoBitmap> _inMemoryBitmaps = new HashSet<GeoBitmap>();

        void IBaseLayerViewer.SetBaseLayer(IBaseLayer baseLayer)
        {
            if (baseLayer == null)
                return;

            else if (_baseLayer == null || _baseLayer.Name != baseLayer.Name)
            {
                _baseLayer = baseLayer;
                if (_IBaseLayerViewer.Enabled)
                {
                    this.Reload();
                }
            }
        }

        bool IBaseLayerViewer.Enabled
        {
            get { return _isBaseLayerEnabled; }
            set
            {
                if (_isBaseLayerEnabled != value)
                {
                    _isBaseLayerEnabled = value;
                    this.Reload();
                }
            }
        }

        bool IBaseLayerViewer.ShowLabels
        {
            get
            {
                return _showLabels;
            }
            set
            {
                if (_showLabels != value)
                {
                    _showLabels = value;
                    this.Reload();
                }
            }
        }

        private void Reload()
        {
            this.Internal_SetGeometry(_geometriesCache);
        }

        float IBaseLayerViewer.Opacity
        {
            get { return _opacity; }
            set
            {
                float newValue = Math.Min(1f, Math.Max(0f, value));
                if (_opacity != newValue)
                {
                    _opacity = newValue;
                    this.InvalidateWrapper(true, false);
                }
            }
        }

        private IBaseLayerViewer _IBaseLayerViewer
        {
            get { return (IBaseLayerViewer)this; }
        }

        private void DrawBaseLayer(Matrix matrix, Graphics g)
        {
            TilePlacementCache _tilePlacement = new TilePlacementCache();
            //foreach (Image img in GetImageTiles(this.ViewBounds, map.ActualWidth, map.ActualHeight))
            int numTilesTotal = 0;
            int numTilesDisc = 0;
            int numTilesInternet = 0;
            int numTilesNull = 0;
            int numTilesMemory = 0;
            List<GeoBitmap> v_listImages = GetImageTiles(matrix);
            foreach (GeoBitmap img in v_listImages)
            {
                try
                {
                    if (img == null || img.Bitmap == null)
                    {
                        numTilesNull++;
                    }
                    else
                    {
                        switch (img.Origin)
                        {
                            case TileOrigin.Disk: numTilesDisc++; break;
                            case TileOrigin.Download: numTilesInternet++; break;
                            case TileOrigin.Memory: numTilesMemory++; break;
                        }

                        PointF hg = new PointF((float)img.BBox.XMin, (float)img.BBox.YMax);
                        PointF hgLogical = GeoToLogical(matrix, hg);
                        PointF bd = new PointF((float)img.BBox.XMax, (float)img.BBox.YMin);
                        PointF bdLogical = GeoToLogical(matrix, bd);
                        float width = bdLogical.X - hgLogical.X;
                        float height = bdLogical.Y - hgLogical.Y;

                        if (width > 0 && height > 0)
                        {
                            TilePlacement placement = _tilePlacement.Suggest(img.Index.X, img.Index.Y, (int)Math.Round(hgLogical.X, 0), (int)Math.Round(hgLogical.Y, 0), (int)Math.Ceiling(width), (int)Math.Ceiling(height));

                            if (_opacity < 1f)
                            {
                                using (Image image = img.Bitmap.SetImageOpacity(_opacity))
                                {
                                    g.DrawImage(image, placement.PosX, placement.PosY, placement.Width, placement.Height);
                                }
                            }
                            else
                            {
                                //Trace.TraceInformation("Image {0} at X: {1}, Y: {2} ({3} x {4})", img.TileInfo, hgLogical.X, bdLogical.Y, width, height);
                                g.DrawImage(img.Bitmap, placement.PosX, placement.PosY, placement.Width, placement.Height);
                            }
                        }
                        // No dispose here, as the memory cache handles image disposing
                        //img.Dispose(); 
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("DrawBaseLayer: " + ex.Message);
                    numTilesNull++;
                    break;
                }
                numTilesTotal++;
            }
            Trace.TraceInformation("{0} tiles processed. Null: {1}, Disk: {2}, Downloaded: {3}, Memory: {4}", numTilesTotal, numTilesNull, numTilesDisc, numTilesInternet, numTilesMemory);
        }


        public PointF LogicalToGeo(Matrix matrix, PointF pt)
        {
            if (matrix.IsInvertible)
            {
                using (Matrix inverseMatrix = matrix.Clone())
                {
                    inverseMatrix.Invert();
                    var points = new PointF[] { pt };
                    inverseMatrix.TransformPoints(points);
                    return points.First();
                }
            }
            throw new ArgumentOutOfRangeException();
        }
        public PointF GeoToLogical(Matrix matrix, PointF pt)
        {
            double px, py = 0;
            BingMapsTileSystem.LatLongToDoubleXY(pt.Y, pt.X, out px, out py);
            pt.X = (float)px;
            pt.Y = (float)py;
            var points = new PointF[] { pt };
            matrix.TransformPoints(points);
            return points.First();
        }

        public BoundingBox GetViewBounds(Matrix matrix)
        {
            PointF topLeft = LogicalToGeo(matrix, new PointF(0, 0));
            PointF rightBottom = LogicalToGeo(matrix, new PointF(this.ClientSize.Width - 1, this.ClientSize.Height - 1));
            BoundingBox bbox = new BoundingBox(topLeft.X, rightBottom.X, rightBottom.Y, topLeft.Y);
            return bbox;
        }

        private double[] LogicalToGeoLatLon(double x, double y)
        {
            double v_x = x;
            double v_y = y;

            double v_latitude = 90 - 360 * Math.Atan(Math.Exp(-v_y * 2 * Math.PI)) / Math.PI;
            double v_longitude = 360 * v_x;

            return new double[] { v_longitude, v_latitude };
        }

        private List<GeoBitmap> GetImageTiles(Matrix matrix)
        {
            List<GeoBitmap> v_ret = new List<GeoBitmap>();

            BoundingBox viewBounds = GetViewBounds(matrix);
            double[] hgLog = LogicalToGeoLatLon(viewBounds.XMin, viewBounds.YMax);
            double[] bdLog = LogicalToGeoLatLon(viewBounds.XMax, viewBounds.YMin);
            BoundingBox viewPortBbox = new BoundingBox(hgLog[0], bdLog[0]
                                                                                                , bdLog[1], hgLog[1]);

            BoundingBox bbox = matrix.Transform(_geomBBox);

            // Get current zoom level
            double mapSizeAtCurrentZoom = 1d * bbox.Width / _geomBBox.Width;

            int zoom = 0;
            uint size = 0;
            while (size <= mapSizeAtCurrentZoom)
            {
                zoom++;
                if (zoom == 24) break;
                size = BingMapsTileSystem.MapSize(zoom);
            }

            // At viewport zoom, what tile size is it ?
            double tileSizeAtZoom = 256 * mapSizeAtCurrentZoom / size;
            Trace.TraceInformation("tileSizeAtZoom: " + tileSizeAtZoom.ToString());
            if (tileSizeAtZoom < 256 && !_baseLayer.UseLowResTiles) Trace.TraceWarning("Error in zoom calculation, tile size should be <256 but it is " + tileSizeAtZoom);
            bool bTakeLowerDef = tileSizeAtZoom < 400;// _baseLayer.UseLowResTiles;
            if (bTakeLowerDef)
            {
                zoom--;
                size = BingMapsTileSystem.MapSize(zoom);
            }



            // Contruct image list
            int startX, startY, endX, endY = 0;
            int tileStartX, tileStartY, tileEndX, tileEndY = 0;
            BingMapsTileSystem.LatLongToPixelXY(viewPortBbox.YMax, viewPortBbox.XMin, zoom, out startX, out startY);
            BingMapsTileSystem.LatLongToPixelXY(viewPortBbox.YMin, viewPortBbox.XMax, zoom, out endX, out endY);
            BingMapsTileSystem.PixelXYToTileXY(startX, startY, out tileStartX, out tileStartY);
            BingMapsTileSystem.PixelXYToTileXY(endX, endY, out tileEndX, out tileEndY);

            bool stop = false;
            for (int x = tileStartX; x <= tileEndX; x++)
            {
                for (int y = tileStartY; y <= tileEndY; y++)
                {
                    if (stop) break;
                    //GeoBitmap geoBmp = await _tileDownloader.DownloadTileAsync(zoom, x, y, _baseLayer);
                    GeoBitmap geoBmp = _tileDownloader.DownloadTile(zoom, x, y, _baseLayer);

                    stop = geoBmp != null && geoBmp.Exception != null && _baseLayer.StopDownloadBatchIfException;

                    if (!stop)
                        v_ret.Add(geoBmp);

                }
                if (stop) break;
            }

            return v_ret;
        }

        #endregion
    }
}

