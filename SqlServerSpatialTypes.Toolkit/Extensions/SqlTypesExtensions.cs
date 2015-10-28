using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

namespace SqlServerSpatialTypes.Toolkit
{
	public static partial class SqlTypesExtensions
	{
		private const double INVALIDGEOM_BUFFER = 0.000001d;
		private const double INVALIDGEOM_REDUCE = 0.00000025d;
		private const double AIRE_MINI_SCORIES = 250d;

		#region Make valid
		public static SqlGeometry MakeValidIfInvalid(this SqlGeometry geom, int? retainDimension = null, double minimumRatio = 0.000001)
		{
			if (geom == null || geom.IsNull)
				return geom;

			SqlGeometry retGeom = null;
			try
			{
				// Makve valid the classic way (otherwise any further manipulation will throw an exception)
				if (geom.STIsValid().IsFalse)
					geom = geom.MakeValid();

				// Minimum acceptable surface
				double v_areaMin = geom.STArea().Value * minimumRatio;
				// Minimum acceptable length
				double v_lengthMin = geom.STLength().Value * minimumRatio;

				int numGeoms = geom.STNumGeometries().Value;

				// Init list of retained geoms
				IEnumerable<SqlGeometry> v_geomsToKeep = geom.Geometries();

				// 1. Filter on dimensions
				if (retainDimension.HasValue)
				{
					v_geomsToKeep = v_geomsToKeep.Where(g => g.STDimension().Value >= retainDimension.Value);
				}

				// 2. Filter on ratios
				v_geomsToKeep = v_geomsToKeep.Where(g => IsSqlGeometryRatioOk(g, v_areaMin, v_lengthMin));

				// 3. Aggregate retained geometries
				retGeom = v_geomsToKeep.AggregateSqlGeometry();

			}
			catch (Exception)
			{
				throw;
			}
			return retGeom;
		}

		private static bool IsSqlGeometryRatioOk(SqlGeometry geom, double minArea, double minLength)
		{
			bool v_ret = false;
			switch (geom.STDimension().Value)
			{
				case 0: // POINT
					v_ret = true;
					break;

				case 1: // Line : check ratio on length
					double v_longueur = geom.STLength().Value;
					if (v_longueur >= minLength)
					{
						v_ret = true;
					}
					break;

				default: // Polygon : check ratio surface

					double v_surf = geom.STArea().Value;
					if (v_surf >= minArea)
					{
						return true;
					}
					break;
			}

			return v_ret;
		}

		public static SqlGeometry AggregateSqlGeometry(this IEnumerable<SqlGeometry> geometries)
		{
			return GeometryAggregateSink.AggregateSqlGeometry(geometries);
		}

		public static OpenGisGeometryType GetSqlGeometryCollectionTypeFromList(this IEnumerable<SqlGeometry> p_ListGeometries)
		{
			// List of distinct types encountered
			List<string> v_distinctValues = p_ListGeometries.Select(g => g.STGeometryType().Value).Distinct().ToList();

			// If several types => GEOMETRYCOLLECTION
			if (v_distinctValues.Count > 1)
			{
				return OpenGisGeometryType.GeometryCollection;
			}
			else
			{
				if (v_distinctValues.First() == "LineString") // Only LINESTRINGs => MULTILINESTRING
					return OpenGisGeometryType.MultiLineString;
				else if (v_distinctValues.First() == "Point") // Only POINTs => MULTIPOINT
					return OpenGisGeometryType.MultiPoint;
				else
					return OpenGisGeometryType.MultiPolygon;		// Only POLYGONs => MULTIPOLYGON
			}
		}



		#endregion

		public static bool TryToGeography(this SqlGeometry geom, out SqlGeography outputGeography)
		{
			try
			{
				geom = geom.MakeValidIfInvalid();

				outputGeography = SqlGeography.STGeomFromText(new SqlChars(new SqlString(geom.ToString())), 4326);
				return true;
			}
			catch
			{
				outputGeography = null;
				return false;
			}
		}
		public static bool TryToGeometry(this SqlGeography geog, out SqlGeometry outputGeometry)
		{
			try
			{
				outputGeometry = SqlGeometry.STGeomFromText(new SqlChars(new SqlString(geog.ToString())), 4326);
				outputGeometry = outputGeometry.MakeValidIfInvalid();
				return true;
			}
			catch
			{
				outputGeometry = null;
				return false;
			}
		}

		public static SqlGeography ToGeography(this SqlGeometry geom)
		{
			try
			{
				geom = geom.MakeValidIfInvalid();

				SqlGeography geog = null;
				if (geom.TryToGeography(out geog))
				{
					return geog;
				}

				// En cas d'échec, on appelle STBuffer avec un param faible puis on le réapelle avec le même param négatif
				// Cela ne change pas la geom mais corrige les erreurs.
				// Source : http://www.beginningspatial.com/fixing_invalid_geography_data
				SqlGeometry v_geomBuffered = geom.STBuffer(INVALIDGEOM_BUFFER).STBuffer(-INVALIDGEOM_BUFFER).Reduce(INVALIDGEOM_REDUCE);
				if (v_geomBuffered.TryToGeography(out geog))
				{
					return geog;
				}

				// Inverse buffer
				v_geomBuffered = geom.STBuffer(-INVALIDGEOM_BUFFER).STBuffer(INVALIDGEOM_BUFFER).Reduce(INVALIDGEOM_REDUCE);
				if (v_geomBuffered.TryToGeography(out geog))
				{
					return geog;
				}

				throw new ArgumentException("La géométrie ne peut pas être convertie en géographie");
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static double GetAireEnMetres(this SqlGeometry geom)
		{
			try
			{
				SqlGeography geog = geom.ToGeography();
				double area = geog.STArea().Value;
				return area;
			}
			catch (Exception)
			{
				throw;
			}
		}


		public static SqlGeometry STGeomFromText(string wkt, int srid)
		{
			return SqlGeometry.STGeomFromText(new SqlChars(new SqlString(wkt)), srid);
		}

		public static SqlGeography STGeogFromText(string wkt, int srid)
		{
			return SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), srid);
		}

		public static SqlGeometry PolygonFromRings(SqlGeometry outerRing, List<SqlGeometry> holes)
		{
			// Check si les parametres sont des LINESTRING
			#region Check params
			if (outerRing == null || outerRing.IsNull)
				throw new ArgumentException("La boucle extérieure est null", "outerRing");

			if (outerRing.STGeometryType().Value != OpenGisGeometryType.LineString.ToString())
				throw new ArgumentException("La boucle extérieure doit être un LINESTRING", "outerRing");
			if (holes != null)
			{
				foreach (var hole in holes)
				{
					if (hole.STGeometryType().Value != OpenGisGeometryType.LineString.ToString())
						throw new ArgumentException("Les boucles intérieures doivent être un LINESTRING", "holes");
				}
			}
			#endregion


			StringBuilder sb = new StringBuilder();
			sb.Append("POLYGON (");
			sb.Append(outerRing.ToString().Replace("LINESTRING ", ""));

			if (holes != null)
			{
				foreach (SqlGeometry hole in holes)
				{

					SqlGeometry polyFromHole = PolygonFromRings(hole, null);

					if (SqlTypesExtensions.GetAireEnMetres(polyFromHole) < AIRE_MINI_SCORIES)
						continue;

					//Debug.WriteLine(polyFromHole.STArea().Value);

					sb.Append(",");
					sb.Append(hole.ToString().Replace("LINESTRING ", ""));
				}
			}

			sb.Append(")");

			SqlGeometry ret = SqlTypesExtensions.STGeomFromText(sb.ToString(), outerRing.STSrid.Value);
			ret = ret.MakeValidIfInvalid(2);

			return ret;
		}

		/// <summary>
		/// Retourne la liste des boucles extérieures d'un polygone
		/// </summary>
		/// <param name="holeGeom"></param>
		/// <returns></returns>
		public static List<SqlGeometry> ExteriorRingsFromPolygon(SqlGeometry polygon)
		{
			if (polygon == null || polygon.IsNull)
				return new List<SqlGeometry>();

			List<SqlGeometry> ringList = new List<SqlGeometry>();
			for (int index = 1; index <= polygon.STNumGeometries(); index++)
			{
				SqlGeometry curPolygon = polygon.STGeometryN(index);

				if (curPolygon.InstanceOf(OpenGisGeometryType.Polygon.ToString()))
				{
					ringList.Add(curPolygon.STExteriorRing());
				}
				else
					Trace.TraceWarning("ExteriorRingsFromPolygon : current geometry is not a polygon");
			}

			return ringList;
		}

		public static SqlGeometry CorrigerUnionGeometry(SqlGeometry geom, int srid)
		{
			SqlGeometry geomBase = SqlTypesExtensions.STGeomFromText("POINT EMPTY", srid);

			for (int i = 1; i <= geom.STNumGeometries(); i++)
			{
				SqlGeometry curGeom = geom.STGeometryN(i);
				if (curGeom.STDimension().Value == 2)
				{
					SqlGeometry outerRing = curGeom.STExteriorRing();

					List<SqlGeometry> holes = new List<SqlGeometry>();

					for (int hole = 1; hole <= curGeom.STNumInteriorRing(); hole++)
					{
						SqlGeometry holeGeom = SqlTypesExtensions.PolygonFromRings(curGeom.STInteriorRingN(hole), null); // trou converti en polygone
						double aire = holeGeom.GetAireEnMetres();
						if (aire > AIRE_MINI_SCORIES)
						{
							List<SqlGeometry> nativeHoles = SqlTypesExtensions.ExteriorRingsFromPolygon(holeGeom); // polygone corrigé reconverti en linestring
							holes.AddRange(nativeHoles);
						}
					}

					curGeom = SqlTypesExtensions.PolygonFromRings(outerRing, holes);
					geomBase = geomBase.STUnion(curGeom);
				}
			}

			return geomBase;
		}


		public static SqlGeometry PointEmpty_SqlGeometry(int srid)
		{
			return SqlGeometry.STPointFromText(new SqlChars(new SqlString("POINT EMPTY")), srid);
		}
		public static SqlGeography PointEmpty_SqlGeography(int srid)
		{
			return SqlGeography.STPointFromText(new SqlChars(new SqlString("POINT EMPTY")), srid);
		}

		public static bool AreSridEqual(this IEnumerable<SqlGeometry> geometries, out int uniqueSrid)
		{
			List<int> srids = geometries.Select(g => g.STSrid.Value).Distinct().ToList();
			if (srids.Count == 1)
			{
				uniqueSrid = srids[0];
				return true;
			}
			else
			{
				uniqueSrid = 0;
				return false;
			}
		}



		#region Enumerators

		public static IEnumerable<Point> Points(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumPoints(); i++)
			{
				yield return new Point(geom.STPointN(i).STX.Value, geom.STPointN(i).STY.Value);
			}
		}

		public static IEnumerable<SqlGeometry> PointsAsSqlGeometry(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumPoints(); i++)
			{
				yield return geom.STPointN(i);
			}
		}

		public static IEnumerable<SqlGeometry> Geometries(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumGeometries(); i++)
			{
				yield return geom.STGeometryN(i);
			}
		}

		public static IEnumerable<SqlGeometry> InteriorRings(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumInteriorRing(); i++)
			{
				yield return geom.STInteriorRingN(i);
			}
		}

		public static bool HasInteriorRings(this SqlGeometry geom)
		{
			return geom.STNumInteriorRing().Value > 0;
		}

		#endregion

		#region Serialization

		//private void ToFile(SqlGeometry geom)
		//{

		//	// To serialize the hashtable and its key/value pairs,  
		//	// you must first open a stream for writing. 
		//	// In this case, use a file stream.
		//	using (FileStream fs = new FileStream("geom.dat", FileMode.Create))
		//	{

		//		// Construct a BinaryFormatter and use it to serialize the data to the stream.
		//		BinaryFormatter formatter = new BinaryFormatter();
		//		try
		//		{
		//			formatter.Serialize(fs, geom.STAsBinary().Value);
		//		}
		//		catch (SerializationException e)
		//		{
		//			Console.WriteLine("Failed to serialize. Reason: " + e.Message);
		//			throw;
		//		}

		//	}

		/// <summary>
		/// Reads a serialized SqlGeometry
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static SqlGeometry Read(string path)
		{
			SqlGeometry geom = null;
			// To serialize the hashtable and its key/value pairs,  
			// you must first open a stream for writing. 
			// In this case, use a file stream.
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{

				// Construct a BinaryFormatter and use it to serialize the data to the stream.
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					geom = (SqlGeometry)formatter.Deserialize(fs);
				}
				catch (SerializationException e)
				{
					Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
					throw;
				}

			}

			return geom;
		}

		/// <summary>
		/// Reads a serialized List<SqlGeometry>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<SqlGeometry> ReadList(string path)
		{
			List<SqlGeometry> geomList = null;
			// To serialize the hashtable and its key/value pairs,  
			// you must first open a stream for writing. 
			// In this case, use a file stream.
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{

				// Construct a BinaryFormatter and use it to serialize the data to the stream.
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					geomList = (List<SqlGeometry>)formatter.Deserialize(fs);
				}
				catch (SerializationException e)
				{
					Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
					throw;
				}

			}

			return geomList;
		}

		//private void Save(SqlGeometry geom)
		//{

		//	// To serialize the hashtable and its key/value pairs,  
		//	// you must first open a stream for writing. 
		//	// In this case, use a file stream.
		//	using (FileStream fs = new FileStream("geom.dat", FileMode.Create))
		//	{

		//		// Construct a BinaryFormatter and use it to serialize the data to the stream.
		//		BinaryFormatter formatter = new BinaryFormatter();
		//		try
		//		{
		//			formatter.Serialize(fs, geom.STAsBinary().Value);
		//		}
		//		catch (SerializationException e)
		//		{
		//			Console.WriteLine("Failed to serialize. Reason: " + e.Message);
		//			throw;
		//		}

		//	}
		//}


		#endregion
	}
}
