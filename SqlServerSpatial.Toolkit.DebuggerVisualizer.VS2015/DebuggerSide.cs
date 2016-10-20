using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using SqlServerSpatial.Toolkit.Visualizer;

[assembly: System.Diagnostics.DebuggerVisualizer(typeof(DebuggerSideSqlGeometry), typeof(VisualizerObjectSource), Target = typeof(SqlGeometry), Description = "SqlServerSpatial.Toolkit")]
[assembly: System.Diagnostics.DebuggerVisualizer(typeof(DebuggerSideSqlGeography), typeof(VisualizerObjectSource), Target = typeof(SqlGeography), Description = "SqlServerSpatial.Toolkit")]
//[assembly: System.Diagnostics.DebuggerVisualizer(typeof(DebuggerSideSqlGeometryList), typeof(VisualizerObjectSource), Target = typeof(List<SqlGeometry>), Description = "SqlGeometry list Visualizer")]
//[assembly: System.Diagnostics.DebuggerVisualizer(typeof(DebuggerSideSqlGeographyList), typeof(VisualizerObjectSource), Target = typeof(List<SqlGeography>), Description = "SqlGeography list Visualizer")]
namespace SqlServerSpatial.Toolkit.Visualizer
{

    #region SqlGeometry
    public class DebuggerSideSqlGeometry : DebuggerSideBase
	{

		public DebuggerSideSqlGeometry()
		{
		}

		protected override SqlGeometry GetObject(IVisualizerObjectProvider objectProvider)
		{
			return (SqlGeometry)objectProvider.GetObject();
		}	

		/// <summary>
		/// Test method
		/// </summary>
		/// <param name="objectToVisualize"></param>
		public static void TestShowVisualizer(object objectToVisualize)
		{
			VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(DebuggerSideSqlGeometry));
			visualizerHost.ShowVisualizer();
		}
	
	}
	public class DebuggerSideSqlGeometryList : DebuggerSideListBase
	{

		public DebuggerSideSqlGeometryList()
		{
		}

		protected override IEnumerable<SqlGeometry> GetObject(IVisualizerObjectProvider objectProvider)
		{
			return (IEnumerable<SqlGeometry>)objectProvider.GetObject();
		}

		/// <summary>
		/// Test method
		/// </summary>
		/// <param name="objectToVisualize"></param>
		public static void TestShowVisualizer(object objectToVisualize)
		{
			VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(DebuggerSideSqlGeometryList));
			visualizerHost.ShowVisualizer();
		}

	}
	#endregion SqlGeometry

	#region SqlGeography
	public class DebuggerSideSqlGeography : DebuggerSideBase
	{

		public DebuggerSideSqlGeography()
		{
		}

		protected override SqlGeometry GetObject(IVisualizerObjectProvider objectProvider)
		{
			SqlGeography geography = (SqlGeography)objectProvider.GetObject();
			SqlGeometry geometry = null;
			if (geography.TryToGeometry(out geometry))
			{
				return geometry;
			}
			else
			{
				throw new Exception("Cannot cast geography to geometry");
			}
		}

		/// <summary>
		/// Test method
		/// </summary>
		/// <param name="objectToVisualize"></param>
		public static void TestShowVisualizer(object objectToVisualize)
		{
			VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(DebuggerSideSqlGeography));
			visualizerHost.ShowVisualizer();
		}
		
	}
	public class DebuggerSideSqlGeographyList : DebuggerSideListBase
	{

		public DebuggerSideSqlGeographyList()
		{
		}

		protected override IEnumerable<SqlGeometry> GetObject(IVisualizerObjectProvider objectProvider)
		{
			IEnumerable<SqlGeography> geography = (IEnumerable<SqlGeography>)objectProvider.GetObject();
			List<SqlGeometry> geometry = new List<SqlGeometry>();
			foreach(var geog in geography)
			{
				SqlGeometry geom = null;
				if (geog.TryToGeometry(out geom))
				{
					geometry.Add(geom);
				}
			}
			return geometry;
		}

		/// <summary>
		/// Test method
		/// </summary>
		/// <param name="objectToVisualize"></param>
		public static void TestShowVisualizer(object objectToVisualize)
		{
			VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(DebuggerSideSqlGeographyList));
			visualizerHost.ShowVisualizer();
		}

	}
	#endregion SqlGeography
		
}
