using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit
{
	/// <summary>
	/// Sink used to aggregate geometries as a sequence
	/// Purpose : fast STUnion() in contexts where you know geometries does not intersect one with other (ie: accumulating subgeometries of a geometry)
	/// 
	/// Warning : this aggregator is fast because it does not checks for intersections. If geometries intersect, they will overlap in the resulting geometry collection
	/// </summary>
	internal class GeometryAggregateSink : IGeometrySink110
	{
		#region IGeometrySink Membres

		IGeometrySink110 _sink;

		public GeometryAggregateSink(IGeometrySink110 p_Sink)
		{
			_sink = p_Sink;
		}

		void IGeometrySink110.AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
		{
		}

		void IGeometrySink.AddLine(double x, double y, double? z, double? m)
		{
			_sink.AddLine(x, y, z, m);
		}

		void IGeometrySink.BeginFigure(double x, double y, double? z, double? m)
		{
			_sink.BeginFigure(x, y, z, m);
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
		}

		#endregion		

		public static SqlGeometry AggregateSqlGeometry(IEnumerable<SqlGeometry> geometries)
		{
			SqlGeometry v_ret = null;
			try
			{
				if (geometries != null)
				{
					int constrainedCount = geometries.Take(2).Count();

					// if constrainedCount == 0 then the sequence is empty
					// if constrainedCount == 1 then the sequence contains a single element
					// if constrainedCount == 2 then the sequence has more than one element

					if (constrainedCount > 0)
					{
						if (constrainedCount == 1)
						{
							v_ret = geometries.First();
						}
						else
						{

							SqlGeometryBuilder builder = new SqlGeometryBuilder();
							int srid = geometries.First().STSrid.Value; builder.SetSrid(srid);

							GeometryAggregateSink builderSink = new GeometryAggregateSink(builder);

							OpenGisGeometryType v_collectionType = geometries.GetSqlGeometryCollectionTypeFromList();
							builder.BeginGeometry(v_collectionType);
							foreach (SqlGeometry geom in geometries)
							{
								geom.Populate(builderSink);
							}
							builder.EndGeometry();

							v_ret = builder.ConstructedGeometry;

						}
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return v_ret;
		}

	}
}
