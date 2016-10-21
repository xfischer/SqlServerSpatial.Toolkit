using Microsoft.SqlServer.Types;
using System.Collections.Generic;
using System.Windows.Media;

namespace SqlServerSpatial.Toolkit
{
	internal class DummySpatialTrace : ISpatialTrace
	{

		public void Indent(string groupName = null)
		{

		}

		public void TraceGeometry(SqlGeometry geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void TraceGeometry(IEnumerable<SqlGeometry> geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
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
		public void TraceGeometry(SqlGeography geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void TraceGeometry(IEnumerable<SqlGeography> geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
		{

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
