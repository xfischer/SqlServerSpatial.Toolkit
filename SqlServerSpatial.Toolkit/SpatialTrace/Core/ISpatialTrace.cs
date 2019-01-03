using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SqlServerSpatial.Toolkit
{
	internal interface ISpatialTrace : IDisposable
	{
		void Indent(string groupName = null);
		void TraceGeometry(IGeometry geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber);
		void TraceGeometry(IEnumerable<IGeometry> geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber);
		void TraceText(string message, string memberName, string sourceFilePath, int sourceLineNumber);
		void SetFillColor(Color color);
		void SetLineColor(Color color);
		void SetLineWidth(float width);
		void ResetStyle();
		void Unindent();
		string TraceFilePath { get; }
		void Clear();
	}
}
