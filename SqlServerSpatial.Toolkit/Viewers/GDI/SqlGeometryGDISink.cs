using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

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
            foreach (var subGeom in geom.Geometries())
            {
                if (subGeom.IsEmpty)
                    continue;


                BeginGeometry(subGeom.OgcGeometryType);
                
                if (subGeom.OgcGeometryType == OgcGeometryType.Polygon)
                {
                    // Coordinates enumeration gets all coords (exterior ring and interiors)
                    // we need something special for polygons

                    Polygon poly = (Polygon)subGeom;

                    // Exterior ring
                    var firstCoord = poly.ExteriorRing.Points().First();
                    BeginFigure(firstCoord.X, firstCoord.Y, firstCoord.Z, null);
                    foreach (var coord in poly.ExteriorRing.Points().Skip(1))
                    {
                        AddLine(coord.X, coord.Y, coord.Z, null);
                    }
                    EndFigure();

                    if (poly.HasInteriorRings())
                    {
                        foreach (var interiorRing in poly.InteriorRings())
                        {
                            firstCoord = interiorRing.Coordinates.First();
                            BeginFigure(firstCoord.X, firstCoord.Y, firstCoord.Z, null);
                            foreach (var coord in interiorRing.Coordinates.Skip(1))
                            {
                                AddLine(coord.X, coord.Y, coord.Z, null);
                            }
                            EndFigure();
                        }
                    }
                }
                else
                {
                    var firstCoord = subGeom.Coordinates.First();
                    BeginFigure(firstCoord.X, firstCoord.Y, firstCoord.Z, null);
                    foreach (var coord in subGeom.Coordinates.Skip(1))
                    {
                        AddLine(coord.X, coord.Y, coord.Z, null);
                    }
                    EndFigure();
                }

                

                

                EndGeometry();
            }

            EndGeometry();
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
                    _gpFill.AddPolygon(coords);
                }
            }
        }

        OgcGeometryType _curType;
        public void BeginGeometry(OgcGeometryType type)
        {
            _curType = type;
        }

        public void EndGeometry()
        {
        }

        public void SetSrid(int srid)
        {

        }


        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new NotImplementedException();
        }
    }
}
