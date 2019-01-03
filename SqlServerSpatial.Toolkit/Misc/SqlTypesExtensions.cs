using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
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
	public static partial class SqlTypesExtensions
	{
		
		public static IGeometry STGeomFromText(string wkt, int srid)
		{
            WKTReader _wktReader = new WKTReader(new GeometryFactory(new PrecisionModel(PrecisionModels.Floating), srid));
            return _wktReader.Read(wkt);
		}

		
		public static IGeometry PointEmpty_IGeometry(int srid)
		{
            return NetTopologySuite.Geometries.Point.Empty;
		}
		
		public static bool AreSridEqual(this IEnumerable<IGeometry> geometries, out int uniqueSrid)
		{
			HashSet<int> srids = new HashSet<int>(geometries.Where(g=> g!= null && g.IsEmpty == false)
																	.Select(g => g.SRID));
			if (srids.Count == 1)
			{
				uniqueSrid = srids.First();
				return true;
			}
			else
			{
				uniqueSrid = 0;
				return false;
			}
		}
	

		#region Enumerators

		public static IEnumerable<Coordinate> Points(this IGeometry geom)
		{
            return geom.Coordinates;
        }

		public static IEnumerable<IGeometry> PointsAsIGeometry(this IGeometry geom)
		{
            return geom.Coordinates.Select(c => new NetTopologySuite.Geometries.Point(c));

        }

		public static IEnumerable<IGeometry> Geometries(this IGeometry geom)
		{
           
			for (int i = 0; i < geom.NumGeometries; i++)
			{
				yield return geom.GetGeometryN(i);
			}
		}

		public static IEnumerable<IGeometry> InteriorRings(this Polygon geom)
		{

			for (int i = 0; i < geom.NumInteriorRings; i++)
			{
				yield return geom.GetInteriorRingN(i);
			}
		}

		public static bool HasInteriorRings(this IGeometry geom)
		{
            if (geom.OgcGeometryType == OgcGeometryType.Polygon)
                return ((Polygon)geom).NumInteriorRings > 0;

            if (geom.OgcGeometryType == OgcGeometryType.MultiPolygon)
                return ((MultiPolygon)geom).Geometries.Any(g => g.HasInteriorRings());

            return false;
		}

		#endregion

		#region Serialization

		//private void ToFile(IGeometry geom)
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
		/// Reads a serialized IGeometry
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static IGeometry Read(string path)
		{
			IGeometry geom = null;
			// To serialize the hashtable and its key/value pairs,  
			// you must first open a stream for writing. 
			// In this case, use a file stream.
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{

				// Construct a BinaryFormatter and use it to serialize the data to the stream.
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					geom = (IGeometry)formatter.Deserialize(fs);
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
		/// Reads a serialized List<IGeometry>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<IGeometry> ReadList(string path)
		{
			List<IGeometry> geomList = null;
			// To serialize the hashtable and its key/value pairs,  
			// you must first open a stream for writing. 
			// In this case, use a file stream.
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{

				// Construct a BinaryFormatter and use it to serialize the data to the stream.
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					geomList = (List<IGeometry>)formatter.Deserialize(fs);
				}
				catch (SerializationException e)
				{
					Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
					throw;
				}

			}

			return geomList;
		}

		//private void Save(IGeometry geom)
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
