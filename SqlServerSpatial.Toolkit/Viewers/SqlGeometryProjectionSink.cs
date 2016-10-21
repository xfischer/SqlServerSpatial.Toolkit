using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit.BaseLayer;

namespace SqlServerSpatial.Toolkit.Viewers
{
	internal class SqlGeometryProjectionSink : IGeometrySink110
	{
		#region IGeometrySink Membres

		IGeometrySink110 _sink;
		int _outSrid;
		Func<double, double, double[]> _coordTransform;

		public SqlGeometryProjectionSink(IGeometrySink110 p_Sink, int outSrid, Func<double, double, double[]> coordTransform)
		{
			_sink = p_Sink;
			_outSrid = outSrid;
			_coordTransform = coordTransform;
			if (_coordTransform == null)
			{
				_coordTransform = new Func<double, double, double[]>((x, y) => new double[] { x, y });
			}
		}

		void IGeometrySink.AddLine(double x, double y, double? z, double? m)
		{
			double[] proj = _coordTransform(x, y);
			_sink.AddLine(proj[0], proj[1], z, m);
		}

		void IGeometrySink.BeginFigure(double x, double y, double? z, double? m)
		{
			double[] proj = _coordTransform(x, y);
			_sink.BeginFigure(proj[0], proj[1], z, m);
		}

		void IGeometrySink.BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeometry(type);
		}

		void IGeometrySink.EndFigure()
		{
			_sink.EndFigure();
		}

		void IGeometrySink.EndGeometry()
		{
			_sink.EndGeometry();
		}

		void IGeometrySink.SetSrid(int srid)
		{
			_sink.SetSrid(_outSrid);
		}

		#endregion

		public static SqlGeometry ReprojectGeometry(SqlGeometry geom, int srid, Func<double, double, double[]> coordTransform)
		{
			if (geom != null)
			{
				SqlGeometryBuilder builder = new SqlGeometryBuilder();
				SqlGeometryProjectionSink sink = new SqlGeometryProjectionSink(builder, srid, coordTransform);
				geom.Populate(sink);

				return builder.ConstructedGeometry;
			}
			return null;
		}

		public static SqlGeometry ReprojectGeometryToMercator(SqlGeometry geom, int zoomLevel)
		{
			//SqlGeometry testDotSpatial = geom.ReprojectTo(DotSpatial.Projections.KnownCoordinateSystems.Projected.World.Mercatorworld);

			return SqlGeometryProjectionSink.ReprojectGeometry(geom, geom.STSrid.Value, new Func<double, double, double[]>((x, y) => {
				double projX = 0;
				double projY = 0;
				BingMapsTileSystem.LatLongToDoubleXY(y, x, out projX, out projY);
				//System.Diagnostics.Trace.TraceInformation("X: {0} / Y: {1}", projX, projY);
				return new double[] { projX, projY };
			}));
		}

		void IGeometrySink110.AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
		{
			throw new NotImplementedException();
		}
	}
}
