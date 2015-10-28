using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SqlServerSpatialTypes.Toolkit
{
	internal interface ISpatialTrace : IDisposable
	{
		void Indent();
		void TraceGeometry(SqlGeometry geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);
		void TraceGeometry(IEnumerable<SqlGeometry> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);
		void TraceGeometry(SqlGeography  geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);
		void TraceGeometry(IEnumerable<SqlGeography> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);
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
