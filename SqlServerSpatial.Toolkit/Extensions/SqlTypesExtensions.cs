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

namespace SqlServerSpatial.Toolkit
{
	/// <summary>
	/// Useful extensions methods to Sql Server spatial data types
	/// </summary>
	public static partial class SqlTypesExtensions
	{
		private const double INVALIDGEOM_BUFFER = 0.000001d;
		private const double INVALIDGEOM_REDUCE = 0.00000025d;
		private const double AIRE_MINI_SCORIES = 250d;

		#region Make valid
		/// <summary>
		/// Validates a SqlGeometry and avoids artefacts.
		/// </summary>
		/// <param name="geom">SqlGeometry to validate</param>
		/// <param name="retainDimension">If specified and MakeValid() is called on geom, there may be artefacts at lower dimensions. This parameter fixes the minimum dimension required.
		/// For example if geom is a Polygon and you want the resulting geometry to be a polygon, pass 2. Thus all points and lines will be omitted from the resulting geometry.</param>
		/// <param name="minimumRatio">If small geometries are generated, they will be omitted if their area ratio vs geom area is below this value.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Simple geometry aggregator. Warning ! Does not check for intersections between geometries.
		/// It's a fast way to accumulate geometries that do not intersect without calling the heavy STUnion().
		/// </summary>
		/// <param name="geometries"></param>
		/// <returns></returns>
		public static SqlGeometry AggregateSqlGeometry(this IEnumerable<SqlGeometry> geometries)
		{
			return GeometryAggregateSink.AggregateSqlGeometry(geometries);
		}

		/// <summary>
		/// Retrieves the good OGC type for a group of geometries you want to aggregate
		/// </summary>
		/// <param name="p_ListGeometries"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Tries the conversion from SqlGeometry to SqlGeography
		/// </summary>
		/// <param name="geom"></param>
		/// <param name="outputGeography"></param>
		/// <returns></returns>
		/// <returns>true if conversion succedeed, false otherwise</returns>
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
		/// <summary>
		/// Tries the conversion from SqlGeography to SqlGeometry
		/// </summary>
		/// <param name="geog"></param>
		/// <param name="outputGeometry"></param>
		/// <returns>true if conversion succedeed, false otherwise</returns>
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



		private static SqlGeometry STGeomFromText(string wkt, int srid)
		{
			return SqlGeometry.STGeomFromText(new SqlChars(new SqlString(wkt)), srid);
		}

		private static SqlGeography STGeogFromText(string wkt, int srid)
		{
			return SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), srid);
		}


		/// <summary>
		/// Retourne la liste des boucles extérieures d'un polygone
		/// </summary>
		/// <returns></returns>
		private static List<SqlGeometry> ExteriorRingsFromPolygon(SqlGeometry polygon)
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

		/// <summary>
		/// Constructs an empty SqlGeometry point
		/// </summary>
		/// <param name="srid"></param>
		/// <returns></returns>
		public static SqlGeometry PointEmpty_SqlGeometry(int srid)
		{
			return SqlGeometry.STPointFromText(new SqlChars(new SqlString("POINT EMPTY")), srid);
		}

		/// <summary>
		/// Constructs an empty SqlGeography point
		/// </summary>
		/// <param name="srid"></param>
		/// <returns></returns>
		public static SqlGeography PointEmpty_SqlGeography(int srid)
		{
			return SqlGeography.STPointFromText(new SqlChars(new SqlString("POINT EMPTY")), srid);
		}

		/// <summary>
		/// Checks if all SRIDs are equal in geometry list
		/// </summary>
		/// <param name="geometries"></param>
		/// <param name="uniqueSrid">The unique SRID found if applicable.</param>
		/// <returns>True if all SRIDs are the same, false otherwise.</returns>
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

		/// <summary>
		/// Enumerator on points allowing forach(var pt in geom.Points()) instead of for(int i = 1; i &lt; geom.STNumPoints()...
		/// This version outputs Point structs instead of SqlGeometry
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static IEnumerable<Point> Points(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumPoints(); i++)
			{
				yield return new Point(geom.STPointN(i).STX.Value, geom.STPointN(i).STY.Value);
			}
		}
		/// <summary>
		/// Enumerator on points allowing forach(var pt in geom.Points()) instead of for(int i = 1; i &lt; geom.STNumPoints()...
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static IEnumerable<SqlGeometry> PointsAsSqlGeometry(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumPoints(); i++)
			{
				yield return geom.STPointN(i);
			}
		}

		/// <summary>
		/// Enumerator on sub geometries allowing forach(var pt in geom.Geometries()) instead of for(int i = 1; i &lt; geom.STNumGeometries()...
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static IEnumerable<SqlGeometry> Geometries(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumGeometries(); i++)
			{
				yield return geom.STGeometryN(i);
			}
		}

		/// <summary>
		/// Enumerator on interior rings allowing forach(var pt in geom.InteriorRings()) instead of for(int i = 1; i &lt; geom.STNumInteriorRing()...
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static IEnumerable<SqlGeometry> InteriorRings(this SqlGeometry geom)
		{
			for (int i = 1; i <= geom.STNumInteriorRing(); i++)
			{
				yield return geom.STInteriorRingN(i);
			}
		}

		/// <summary>
		/// Checks wether a geometry has interior rings.
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static bool HasInteriorRings(this SqlGeometry geom)
		{
			return geom.STNumInteriorRing().Value > 0;
		}

		#endregion

		#region Serialization

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
		/// Reads a serialized List of SqlGeometry
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

		/// <summary>
		/// Serialize and save a SqlGeometry to a file
		/// </summary>
		/// <param name="geometry">SqlGeometry to save</param>
		/// <param name="fileName">Destination file. If file exists it will be replaced.</param>
		public static void Save(this SqlGeometry geometry, string fileName)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream dataFile = new FileStream(fileName, FileMode.Create))
			{
				formatter.Serialize(dataFile, geometry);
			}
		}

		/// <summary>
		/// Serialize and save a SqlGeometry list to a file
		/// </summary>
		/// <param name="geometry">SqlGeometry list to save</param>
		/// <param name="fileName">Destination file. If file exists it will be replaced.</param>
		public static void Save(this IEnumerable<SqlGeometry> geometryList, string fileName)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream dataFile = new FileStream(fileName, FileMode.Create))
			{
				formatter.Serialize(dataFile, geometryList.ToList());
			}
		}

		#endregion
	}
}
