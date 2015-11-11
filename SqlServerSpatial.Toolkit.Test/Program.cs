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

			//TestTrace();
			TestCentroid();
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
		}

		static void TestCentroid()
		{
			SqlGeometry geometry = SqlGeometry.STPolyFromText(new SqlChars(new SqlString("POLYGON((6.591796875 44.43377984606825,4.1748046875 44.402391829093915,4.21875 42.35854391749705,6.85546875 42.391008609205045,6.591796875 44.43377984606825))")), 4326);
			SqlGeometry centroid = geometry.STCentroid();

			SpatialTrace.TraceGeometry(geometry, "geometry");
			SpatialTrace.SetFillColor(Colors.Red);
			SpatialTrace.TraceGeometry(centroid, "Centroid");
			SpatialTrace.ShowDialog();
		}
	}
}
﻿