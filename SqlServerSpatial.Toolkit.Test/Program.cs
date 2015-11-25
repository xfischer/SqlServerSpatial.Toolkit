using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using System.Windows.Media;

namespace SqlServerSpatial.Toolkit.Test
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

			TestTrace();
			//TestCentroid();
			TestVariousGeometries();
		}

		private static void TestVariousGeometries()
		{
			SqlGeometry simplePoint = SqlGeometry.Point(1, 47, 4326);
			SqlGeometry multiPoint = SqlGeometry.Parse(new SqlString("MULTIPOINT((1 47),(1 46),(0 46),(0 47),(1 47))")); multiPoint.STSrid = 4326;
			SqlGeometry lineString = SqlGeometry.Parse(new SqlString("LINESTRING(1 47,1 46,0 46,0 47,1 47)")); lineString.STSrid = 4326;
			SqlGeometry multiLineString = SqlGeometry.Parse(new SqlString("MULTILINESTRING((0.516357421875 47.6415668949958,0.516357421875 47.34463879017405,0.977783203125 47.22539733216678,1.175537109375 47.463611506072866,0.516357421875 47.6415668949958),(0.764923095703125 47.86549372980948,0.951690673828125 47.82309640371982,1.220855712890625 47.79911736820551,1.089019775390625 47.69015026565801,1.256561279296875 47.656860648589))"));
			multiLineString.STSrid = 4326;
			SqlGeometry simplePoly = SqlGeometry.Parse(new SqlString("POLYGON((1 47,1 46,0 46,0 47,1 47))")); simplePoly.STSrid = 4326;
			SqlGeometry polyWithHole = SqlGeometry.Parse(new SqlString(@"
					POLYGON(
					(0.516357421875 47.6415668949958,0.516357421875 47.34463879017405,0.977783203125 47.22539733216678,1.175537109375 47.463611506072866,0.516357421875 47.6415668949958),
					(0.630340576171875 47.54944962456812,0.630340576171875 47.49380564962583,0.729217529296875 47.482669772098674,0.731964111328125 47.53276262898896,0.630340576171875 47.54944962456812)
					)")); polyWithHole.STSrid = 4326;
			SqlGeometry multiPolygon = SqlGeometry.Parse(new SqlString(@"
					MULTIPOLYGON (
						((40 40, 20 45, 45 30, 40 40)),
						((20 35, 45 20, 30 5, 10 10, 10 30, 20 35), (30 20, 20 25, 20 15, 30 20)),
						((0.516357421875 47.6415668949958,0.516357421875 47.34463879017405,0.977783203125 47.22539733216678,1.175537109375 47.463611506072866,0.516357421875 47.6415668949958),(0.630340576171875 47.54944962456812,0.630340576171875 47.49380564962583,0.729217529296875 47.482669772098674,0.731964111328125 47.53276262898896,0.630340576171875 47.54944962456812))
					)")); multiPolygon.STSrid = 4326;

			SqlGeometry geomCol = SqlGeometry.Parse(new SqlString(@"
					GEOMETRYCOLLECTION (
						POLYGON((0.516357421875 47.6415668949958,0.516357421875 47.34463879017405,0.977783203125 47.22539733216678,1.175537109375 47.463611506072866,0.516357421875 47.6415668949958),(0.630340576171875 47.54944962456812,0.630340576171875 47.49380564962583,0.729217529296875 47.482669772098674,0.731964111328125 47.53276262898896,0.630340576171875 47.54944962456812)),
						LINESTRING(0.764923095703125 47.86549372980948,0.951690673828125 47.82309640371982,1.220855712890625 47.79911736820551,1.089019775390625 47.69015026565801,1.256561279296875 47.656860648589),
						POINT(0.767669677734375 47.817563762851776)
					)")); geomCol.STSrid = 4326;

			SpatialTrace.Enable();
			SpatialTrace.SetFillColor(Color.FromArgb(128, 0, 0, 255)); // Fill with blue
			SpatialTrace.TraceGeometry(simplePoint,"simplePoint");
			SpatialTrace.SetFillColor(Color.FromArgb(128, 255, 0, 0)); // Fill with red
			SpatialTrace.TraceGeometry(multiPoint, "multiPoint");
			SpatialTrace.ResetStyle();
			SpatialTrace.TraceGeometry(lineString, "lineString");
			SpatialTrace.TraceGeometry(multiLineString, "multiLineString");
			SpatialTrace.TraceGeometry(simplePoly, "simplePoly");
			SpatialTrace.TraceGeometry(polyWithHole, "polyWithHole");
			SpatialTrace.TraceGeometry(multiPolygon, "multiPolygon");
			SpatialTrace.TraceGeometry(geomCol, "geomCol");
			SpatialTrace.ShowDialog();
			SpatialTrace.Clear();
		}

		private static void TestTrace()
		{
			// Test with SqlGeometry
			SqlGeometry geometry = SqlGeometry.STPolyFromText(new SqlChars(new SqlString("POLYGON((6.591796875 44.43377984606825,4.1748046875 44.402391829093915,4.21875 42.35854391749705,6.85546875 42.391008609205045,6.591796875 44.43377984606825))")), 4326);

			SqlGeometry geometryBuf = geometry.STBuffer(0.2);
			SqlGeometry geometryBuf2 = geometry.STBuffer(-0.2);

			// Test with SqlGeography
			SqlGeography geography = SqlGeography.STPolyFromText(new SqlChars(new SqlString("POLYGON((6.591796875 44.43377984606825,4.1748046875 44.402391829093915,4.21875 42.35854391749705,6.85546875 42.391008609205045,6.591796875 44.43377984606825))")), 4326);


			SpatialTrace.Enable();
			SpatialTrace.TraceGeometry(geometry, "Sample geometry with default style");
			SpatialTrace.SetLineWidth(3); // 3 pixels wide stroke
			SpatialTrace.Indent();
			SpatialTrace.TraceGeometry(geometryBuf, "Positive buffer");
			SpatialTrace.TraceGeometry(geometryBuf2, "Negative buffer");
			SpatialTrace.SetLineWidth(1); // 1 pixel wide stroke
			SpatialTrace.SetFillColor(Color.FromArgb(128, 255, 0, 0)); // Fill with red
			SpatialTrace.SetLineColor(Color.FromArgb(128, 0, 0, 255)); // Blue stroke
			SpatialTrace.Indent();
			SpatialTrace.TraceGeometry(geometry, "Sample geometry with custom style");
			SpatialTrace.SetFillColor(Color.FromArgb(128, 0, 0, 255)); // Fill with blue
			SpatialTrace.SetLineColor(Color.FromArgb(128, 0, 255, 0)); // Red stroke
			SpatialTrace.TraceGeometry(geography, "Sample geography");
			SpatialTrace.Unindent();
			SpatialTrace.Unindent();
			SpatialTrace.ShowDialog();
			SpatialTrace.Clear();
		}

		static void TestCentroid()
		{
			SqlGeometry geometry = SqlGeometry.STPolyFromText(new SqlChars(new SqlString("POLYGON((6.591796875 44.43377984606825,4.1748046875 44.402391829093915,4.21875 42.35854391749705,6.85546875 42.391008609205045,6.591796875 44.43377984606825))")), 4326);
			SqlGeometry centroid = geometry.STCentroid();

			SpatialTrace.TraceGeometry(geometry, "geometry");
			SpatialTrace.SetFillColor(Colors.Red);
			SpatialTrace.TraceGeometry(centroid, "Centroid");
			SpatialTrace.ShowDialog();
			SpatialTrace.Clear();
		}
	}
}
﻿