using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SqlServerSpatialTypes.Toolkit
{
	/// <summary>
	/// Converts Sql geometries to WPF Paths
	/// </summary>
	public static partial class SqlTypesExtensions
	{
		/// <summary>
		/// Converts a sql geometry instance to a WPF Path instance
		/// </summary>
		/// <param name="geom"></param>
		/// <param name="fill"></param>
		/// <param name="stroke"></param>
		/// <param name="strokeThickness"></param>
		/// <param name="unitVector"></param>
		/// <returns></returns>
		public static Path ToShapeWpf(this SqlGeometry geom, Brush fill, Brush stroke, double strokeThickness, Vector unitVector)
		{
			Path path = new Path();
			path.Stroke = stroke;
			path.StrokeThickness = strokeThickness;

			GeometryGroup group = new GeometryGroup();
			group.FillRule = FillRule.Nonzero;

			switch (geom.STGeometryType().ToString())
			{
				case "Polygon":

					group.Children.Add(ConvertSimpleGeometry(geom));
					path.Fill = fill;
					break;

				case "MultiPolygon":

					foreach (SqlGeometry part in geom.Geometries())
					{
						group.Children.Add(ConvertSimpleGeometry(part));
					}
					path.Fill = fill;
					break;

				case "LineString":

					group.Children.Add(ConvertSimpleGeometry(geom));
					break;

				case "MultiLineString":

					foreach (SqlGeometry part in geom.Geometries())
					{
						group.Children.Add(ConvertSimpleGeometry(part));
					}

					break;

				case "GeometryCollection":

					foreach (SqlGeometry part in geom.Geometries())
					{
						group.Children.Add(ConvertSimpleGeometry(part));
					}
					path.Fill = fill;

					break;
				case "Point":

					group.Children.Add(ConvertSimpleGeometry(geom, unitVector));
					path.Fill = fill;
					break;

				case "MultiPoint":

					foreach (SqlGeometry part in geom.Geometries())
					{
						Geometry g = ConvertSimpleGeometry(part, unitVector);
						group.Children.Add(g);
					}
					path.Fill = fill;
					break;

				default:

					throw new NotSupportedException(string.Format("Geometry type {0} not supported", geom.STGeometryType()));
			}

			path.Data = group;

			return path;
		}

		private static Geometry ConvertSimpleGeometry(SqlGeometry geom, Vector unitVector = default(Vector))
		{
			Geometry ret = null;
			try
			{
				switch (geom.STGeometryType().ToString())
				{
					case "Polygon":

						ret = ConvertPolygon(geom);
						break;

					case "LineString":

						ret = ConvertLineString(geom);
						break;

					case "Point":

						ret = ConvertPoint(geom, unitVector);
						break;
					default:

						throw new NotSupportedException(string.Format("ConvertSimpleGeometry: Geometry type {0} not supported", geom.STGeometryType()));
				}
			}
			catch (Exception)
			{
				throw;
			}

			return ret;

		}

		private static Geometry ConvertPoint(SqlGeometry geom, Vector unitVector)
		{
			EllipseGeometry pointEllipse = new EllipseGeometry(new Point(geom.STX.Value, geom.STY.Value), unitVector.Length*2d, unitVector.Length*2d);
			return pointEllipse;

			//PathGeometry pathGeom = new PathGeometry();
			//pathGeom.FillRule = FillRule.EvenOdd;
			////pathGeom.Figures.Add(pointEllipse);

			//double x = geom.STX.Value;
			//double y = geom.STY.Value;
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

		private static Geometry ConvertPolygon(SqlGeometry geom)
		{
			PathGeometry pathGeom = new PathGeometry();
			pathGeom.FillRule = FillRule.EvenOdd;

			// ExteriorRing
			PathFigure extRing = ConvertRing(geom.STExteriorRing());
			pathGeom.Figures.Add(extRing);

			if (geom.HasInteriorRings())
			{
				foreach (var ring in geom.InteriorRings())
				{
					pathGeom.Figures.Add(ConvertRing(ring));
				}
			}
			return pathGeom;
		}

		private static PathFigure ConvertRing(SqlGeometry ring)
		{
			IEnumerable<PathSegment> segments = ring.Points()
																										.Skip(1)
																										.Select(pt => ((PathSegment)new LineSegment(pt, true)));
			PathFigure pathFigure = new PathFigure(ring.Points().First(), segments, false);
			return pathFigure;
		}

		private static Geometry ConvertLineString(SqlGeometry lineString)
		{
			return new PathGeometry(new List<PathFigure>() { ConvertRing(lineString) });
		}


	}
}
