using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotSpatial.Projections;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit
{
	public static class SqlGeometryReprojection
	{

		public static SqlGeometry ReprojectTo(this SqlGeometry geom, int destinationEpsgCode)
		{
			Func<double[], double[]> reprojectionFunc = SqlGeometryReprojection.Identity;

			if (geom.STSrid.Value != destinationEpsgCode)
			{
				// Defines the starting coordiante system
				ProjectionInfo pStart = ProjectionInfo.FromEpsgCode(geom.STSrid.Value);
				// Defines the starting coordiante system
				ProjectionInfo pEnd = ProjectionInfo.FromEpsgCode(destinationEpsgCode);

				reprojectionFunc = pts => SqlGeometryReprojection.ReprojectPoint(pts, 0, pStart, pEnd);
			}

			GeometryToGeometrySink sink = new GeometryToGeometrySink(destinationEpsgCode, pts => reprojectionFunc(pts));
			geom.Populate(sink);


			return sink.ConstructedGeometry;
		}

		public static SqlGeometry ReprojectTo(this SqlGeometry geom, ProjectionInfo destination)
		{
			Func<double[], double[]> reprojectionFunc = SqlGeometryReprojection.Identity;

			// Defines the starting coordiante system
			ProjectionInfo pStart = ProjectionInfo.FromEpsgCode(geom.STSrid.Value);
			// Defines the starting coordiante system
			ProjectionInfo pEnd = destination;

			reprojectionFunc = pts => SqlGeometryReprojection.ReprojectPoint(pts, 0, pStart, pEnd);

			GeometryToGeometrySink sink = new GeometryToGeometrySink(destination.AuthorityCode, pts => reprojectionFunc(pts));
			geom.Populate(sink);

			return sink.ConstructedGeometry;
		}

		private static double[] ReprojectPoint(double[] sourcePoint, double z, ProjectionInfo sourceProj, ProjectionInfo destProj)
		{
			// Calls the reproject function that will transform the input location to the output locaiton
			Reproject.ReprojectPoints(sourcePoint, new double[] { z }, sourceProj, destProj, 0, 1);

			return sourcePoint;
		}

		private static double[] Identity(double[] sourcePoint)
		{
			return sourcePoint;
		}

	}

	internal class GeometryToGeometrySink : IGeometrySink110
	{
		#region IGeometrySink Membres

		private readonly SqlGeometryBuilder _builder;
		private Func<double[], double[]> _reprojectionFunc;

		public GeometryToGeometrySink(int srid, Func<double[], double[]> reprojectionFunc)
		{
			_builder = new SqlGeometryBuilder();
			_builder.SetSrid(srid);
			_reprojectionFunc = reprojectionFunc;
		}

		void IGeometrySink.AddLine(double x, double y, double? z, double? m)
		{
			double[] pts = new double[] { x, y };
			pts = _reprojectionFunc(pts);
			_builder.AddLine(pts[0], pts[1]);
		}

		void IGeometrySink.BeginFigure(double x, double y, double? z, double? m)
		{
			double[] pts = new double[] { x, y };
			pts = _reprojectionFunc(pts);
			_builder.BeginFigure(pts[0], pts[1]);
		}

		void IGeometrySink.BeginGeometry(OpenGisGeometryType type)
		{
			_builder.BeginGeometry(type);
		}

		void IGeometrySink.EndFigure()
		{
			_builder.EndFigure();
		}

		void IGeometrySink.EndGeometry()
		{
			_builder.EndGeometry();
		}

		void IGeometrySink.SetSrid(int srid)
		{

		}
		void IGeometrySink110.AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
		{
			throw new NotImplementedException();
		}

		#endregion

		public SqlGeometry ConstructedGeometry
		{
			get
			{
				return _builder.ConstructedGeometry.MakeValidIfInvalid();
			}
		}
	}
}
