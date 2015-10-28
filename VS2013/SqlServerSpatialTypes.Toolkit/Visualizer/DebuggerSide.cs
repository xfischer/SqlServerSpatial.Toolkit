using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using SqlServerSpatialTypes.Toolkit.Visualizer;
using System.Runtime.Serialization;
using System.IO;

[assembly: System.Diagnostics.DebuggerVisualizer(typeof(DebuggerSideSqlGeometry), typeof(VisualizerObjectSource), Target = typeof(SqlGeometry), Description = "SqlGeometry Visualizer")]
[assembly: System.Diagnostics.DebuggerVisualizer(typeof(DebuggerSideSqlGeography), typeof(VisualizerObjectSource), Target = typeof(SqlGeography), Description = "SqlGeography Visualizer")]
namespace SqlServerSpatialTypes.Toolkit.Visualizer
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
	#endregion SqlGeography
		
}
