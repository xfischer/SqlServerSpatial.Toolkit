using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetTopologySuite.Diagnostics
{
	/// <summary>
	/// WPF
	/// </summary>
	public static partial class SqlTypesExtensions
	{
		public static Path ToShapeWpf(this IGeometry geom, Brush fill, Brush stroke, double strokeThickness, Vector unitVector)
		{
			Path path = new Path();
			path.Stroke = stroke;
			path.StrokeThickness = strokeThickness;

			GeometryGroup group = new GeometryGroup();
			group.FillRule = FillRule.Nonzero;

			switch (geom.OgcGeometryType)
			{
                case OgcGeometryType.Polygon:

					group.Children.Add(ConvertSimpleGeometry(geom));
					path.Fill = fill;
					break;

                case OgcGeometryType.MultiPolygon:

                    foreach (IGeometry part in geom.Geometries())
					{
						group.Children.Add(ConvertSimpleGeometry(part));
					}
					path.Fill = fill;
					break;

                case OgcGeometryType.LineString:

                    group.Children.Add(ConvertSimpleGeometry(geom));
					break;

				case OgcGeometryType.MultiLineString:

					foreach (IGeometry part in geom.Geometries())
					{
						group.Children.Add(ConvertSimpleGeometry(part));
					}

					break;

				case OgcGeometryType.GeometryCollection:

					foreach (IGeometry part in geom.Geometries())
					{
						group.Children.Add(ConvertSimpleGeometry(part));
					}
					path.Fill = fill;

					break;
				case OgcGeometryType.Point:

					group.Children.Add(ConvertSimpleGeometry(geom, unitVector));
					path.Fill = fill;
					break;

				case OgcGeometryType.MultiPoint:

					foreach (IGeometry part in geom.Geometries())
					{
						Geometry g = ConvertSimpleGeometry(part, unitVector);
						group.Children.Add(g);
					}
					path.Fill = fill;
					break;

				default:

					throw new NotSupportedException(string.Format("Geometry type {0} not supported", geom.OgcGeometryType));
			}

			path.Data = group;

			return path;
		}

		private static Geometry ConvertSimpleGeometry(IGeometry geom, Vector unitVector = default(Vector))
		{
			Geometry ret = null;
			try
			{
				switch (geom.OgcGeometryType)
				{
					case OgcGeometryType.Polygon:

						ret = ConvertPolygon(geom);
						break;

					case OgcGeometryType.LineString:

						ret = ConvertLineString((NetTopologySuite.Geometries.LineString)geom);
						break;

					case OgcGeometryType.Point:

						ret = ConvertPoint(geom, unitVector);
						break;
					default:

						throw new NotSupportedException(string.Format("ConvertSimpleGeometry: Geometry type {0} not supported", geom.OgcGeometryType));
				}
			}
			catch (Exception)
			{
				throw;
			}

			return ret;

		}

		private static Geometry ConvertPoint(IGeometry geom, Vector unitVector)
		{
			EllipseGeometry pointEllipse = new EllipseGeometry(new Point(geom.Coordinate.X, geom.Coordinate.Y), unitVector.Length*2d, unitVector.Length*2d);
			return pointEllipse;

			//PathGeometry pathGeom = new PathGeometry();
			//pathGeom.FillRule = FillRule.EvenOdd;
			////pathGeom.Figures.Add(pointEllipse);

			//double x = geom.X;
			//double y = geom.Y;
			//double l = unitVector.Length*5d;

			//List<PathSegment> paths = new List<PathSegment>();
			//paths.Add(new LineSegment(new Point(x - l, y + l), true));
			//paths.Add(new LineSegment(new Point(x + l, y + l), true));
			//paths.Add(new LineSegment(new Point(x + l, y - l), true));
			//paths.Add(new LineSegment(new Point(x - l, y - l), true));


			//// ExteriorRing
			//PathFigure pointPathFig = new PathFigure(new Point(x - l, y - l), paths, true);
			//pathGeom.Figures.Add(pointPathFig);

			//return pathGeom;
		}

		private static Geometry ConvertPolygon(IGeometry geom)
		{
            NetTopologySuite.Geometries.Polygon poly = (NetTopologySuite.Geometries.Polygon)geom;
            PathGeometry pathGeom = new PathGeometry();
			pathGeom.FillRule = FillRule.EvenOdd;


			// ExteriorRing
			PathFigure extRing = ConvertRing(poly.ExteriorRing);
			pathGeom.Figures.Add(extRing);

			if (geom.HasInteriorRings())
			{
				foreach (var ring in poly.InteriorRings)
				{
					pathGeom.Figures.Add(ConvertRing(ring));
				}
			}
			return pathGeom;
		}

		private static PathFigure ConvertRing(ILineString ring)
		{
			IEnumerable<PathSegment> segments = ring.Points()
																										.Skip(1)
																										.Select(pt => ((PathSegment)new LineSegment(new Point(pt.X,pt.Y), true)));
			PathFigure pathFigure = new PathFigure(new Point(ring.Coordinates.First().X, ring.Coordinates.First().Y), segments, false);
			return pathFigure;
		}

		private static Geometry ConvertLineString(ILineString lineString)
		{
			return new PathGeometry(new List<PathFigure>() { ConvertRing(lineString) });
		}


	}
}
