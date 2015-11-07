using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SqlServerSpatialTypes.Toolkit
{
	internal interface ISpatialTrace : IDisposable
	{
		void Indent();
		/// <summary>
		/// Adds a SqlGeometry to trace file.
		/// </summary>
		/// <param name="geom">SqlGeometry instance you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		void TraceGeometry(SqlGeometry geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);

		/// <summary>
		/// Adds a list of SqlGeometry to trace file.
		/// </summary>
		/// <param name="geom">SqlGeometry instances you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		void TraceGeometry(IEnumerable<SqlGeometry> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);

		/// <summary>
		/// Adds a SqlGeography to trace file.
		/// </summary>
		/// <param name="geom">SqlGeography instance you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		void TraceGeometry(SqlGeography  geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);

		/// <summary>
		/// Adds a list of SqlGeography to trace file.
		/// </summary>
		/// <param name="geom">SqlGeography instances you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		void TraceGeometry(IEnumerable<SqlGeography> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber);

		/// <summary>
		/// Adds informative text to the trace file.
		/// </summary>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		void TraceText(string message, string memberName, string sourceFilePath, int sourceLineNumber);

		/// <summary>
		/// Changes current fill color. All subsequent calls will be drawn with the specified color
		/// </summary>
		/// <param name="color"></param>
		void SetFillColor(Color color);
		/// <summary>
		/// Changes current line color. All subsequent calls will be drawn with the specified color
		/// </summary>
		/// <param name="color"></param>
		void SetLineColor(Color color);
		/// <summary>
		/// Changes current line width. All subsequent calls will be drawn with the specified width
		/// </summary>
		/// <param name="width"></param>
		void SetLineWidth(float width);

		/// <summary>
		/// Reset drawing style to defaults
		/// </summary>
		void ResetStyle();
		/// <summary>
		/// Unindents the trace file for clarity. Remember to call <see cref="Unindent">Unindent</see>.
		/// </summary>
		void Unindent();
		/// <summary>
		/// Gets the Trace text file path generated
		/// </summary>
		string TraceFilePath { get; }
		/// <summary>
		/// Clears the output file.
		/// </summary>
		void Clear();
	}
}
