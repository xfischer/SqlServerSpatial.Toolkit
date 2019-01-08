using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows;
using GeoAPI.Geometries;

namespace NetTopologySuite.Diagnostics.Viewers
{
    /// <summary>
    /// Sinks that fills two graphics paths : one for the filled geometries and one other for the outlines
    /// </summary>
    internal class IGeometryGDISink
    {
        GraphicsPath _gpStroke;
        GraphicsPath _gpFill;
        List<PointF> _currentLine;
        List<PointF> _points;

        public static void ConvertIGeometry(IGeometry geom, ref GraphicsPath stroke, ref GraphicsPath fill, ref List<PointF> points)
        {
            IGeometryGDISink sink = new IGeometryGDISink(stroke, fill, points);
            sink.Populate(geom);
        }

        private void Populate(IGeometry geom)
        {
            BeginGeometry(geom.OgcGeometryType);
            foreach (var subGeom in geom.Geometries().Union(geom.InteriorRings()))
            {
                if (subGeom.IsEmpty)
                    continue;

                if (_curType == OgcGeometryType.Polygon && subGeom.OgcGeometryType ==  OgcGeometryType.LineString)
                {
                    BeginGeometry(OgcGeometryType.Polygon); // force polygon for interior rings
                } else
                {
                    BeginGeometry(subGeom.OgcGeometryType);
                }

                
                var firstCoord = subGeom.Coordinates.First();
                BeginFigure(firstCoord.X, firstCoord.Y, firstCoord.Z, null);
                foreach(var coord in subGeom.Coordinates.Skip(1) )
                {
                    AddLine(coord.X, coord.Y, coord.Z, null);
                }
                EndFigure();
                EndGeometry();
            }
            
            EndGeometry();
        }

        private void EndGeometry()
        {
        }

        private IGeometryGDISink(GraphicsPath gpStroke, GraphicsPath gpFill, List<PointF> points)
        {
            _gpStroke = gpStroke;
            _gpFill = gpFill;
            _currentLine = new List<PointF>();
            _points = points;
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (_curType == OgcGeometryType.Point)
            {
                _points.Add(new PointF((float)x, (float)y));
            }
            else
            {
                _gpStroke.StartFigure();

                _currentLine.Clear();
                _currentLine.Add(new PointF((float)x, (float)y));
            }
        }
        public void AddLine(double x, double y, double? z, double? m)
        {
            _currentLine.Add(new PointF((float)x, (float)y));
        }
        public void EndFigure()
        {
            if (_curType != OgcGeometryType.Point)
            {
                _gpStroke.CloseFigure();


                PointF[] coords = _currentLine.ToArray();
                _gpStroke.AddLines(coords);

                if (_curType == OgcGeometryType.Polygon)
                {
                    _currentLine.Add(_currentLine.First());
                    _gpFill.AddPolygon(coords);
                } 
            }
        }

        OgcGeometryType _curType;
        public void BeginGeometry(OgcGeometryType type)
        {
            _curType = type;
        }
    }
}
