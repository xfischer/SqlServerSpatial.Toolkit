using GeoAPI.Geometries;
using System.Collections.Generic;
using System.Windows.Media;

namespace NetTopologySuite.Diagnostics
{
	internal class DummySpatialTrace : ISpatialTrace
	{

		public void Indent(string groupName = null)
		{

		}

		public void TraceGeometry(IGeometry geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void TraceGeometry(IEnumerable<IGeometry> geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void TraceText(string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void Unindent()
		{

		}

		public void Clear()
		{

		}

		public void Dispose()
		{
		}

		public string TraceFilePath
		{
			get { return null; }
		}
		public string TraceDataDirectory
		{
			get { return null; }
		}
		public void SetFillColor(Color color)
		{

		}
		public void SetLineColor(Color color)
		{

		}
		public void SetLineWidth(float width)
		{
		}


		public void ResetStyle()
		{
		}
	}
}
