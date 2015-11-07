using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Windows.Media;
using System.Diagnostics;

namespace SqlServerSpatialTypes.Toolkit
{
	/// <summary>
	/// Spatial trace static class. Usage is similar to <see cref="System.Diagnostics.Trace">System.Diagnostics.Trace</see>
	/// All trace is written in SpatialTrace.txt file in top-level running assembly directory
	/// </summary>
	public static class SpatialTrace
	{
		private static ISpatialTrace _trace; // trace singleton
		private static ISpatialTrace _dummyTrace;
		private static bool _isEnabled;
		private const string TRACE_DATA_DIR = "SpatialTraceData";
		private const string TRACE_DATA_FILE = "SpatialTrace.txt";

		private static ISpatialTrace Current
		{
			get
			{
				if (_isEnabled)
				{
					if (_trace == null)
					{
						_trace = new SpatialTraceInternal();
					}

					return _trace;
				}
				else
				{
					return _dummyTrace;
				}
			}
		}

		static SpatialTrace()
		{
			try
			{
				_dummyTrace = new DummySpatialTrace();

				_isEnabled = false;
				Boolean.TryParse(ConfigurationManager.AppSettings["EnableSpatialTrace"], out _isEnabled);
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Adds a SqlGeometry to trace file.
		/// </summary>
		/// <param name="geom">SqlGeometry instance you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		public static void TraceGeometry(SqlGeometry geom, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			SpatialTrace.Current.TraceGeometry(geom, message, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Adds a list of SqlGeometry to trace file.
		/// </summary>
		/// <param name="geomList">SqlGeometry instances you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		public static void TraceGeometry(IEnumerable<SqlGeometry> geomList, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			SpatialTrace.Current.TraceGeometry(geomList, message, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Adds a SqlGeography to trace file.
		/// </summary>
		/// <param name="geog">SqlGeography instance you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		public static void TraceGeometry(SqlGeography geog, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			SpatialTrace.Current.TraceGeometry(geog, message, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Adds a list of SqlGeography to trace file.
		/// </summary>
		/// <param name="geogList">SqlGeography instances you want to trace</param>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		public static void TraceGeometry(IEnumerable<SqlGeography> geogList, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			SpatialTrace.Current.TraceGeometry(geogList, message, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Adds informative text to the trace file.
		/// </summary>
		/// <param name="message">Informative message</param>
		/// <param name="memberName">Caller member name. Auto filled by the framework</param>
		/// <param name="sourceFilePath">Auto filled by the framework</param>
		/// <param name="sourceLineNumber">Auto filled by the framework</param>
		public static void TraceText(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			SpatialTrace.Current.TraceText(message, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Changes current fill color. All subsequent calls will be drawn with the specified color
		/// </summary>
		/// <param name="color"></param>
		public static void SetFillColor(Color color)
		{
			SpatialTrace.Current.SetFillColor(color);
		}

		/// <summary>
		/// Changes current line color. All subsequent calls will be drawn with the specified color
		/// </summary>
		/// <param name="color"></param>
		public static void SetLineColor(Color color)
		{
			SpatialTrace.Current.SetLineColor(color);
		}

		/// <summary>
		/// Changes current line width. All subsequent calls will be drawn with the specified width
		/// </summary>
		/// <param name="width"></param>
		public static void SetLineWidth(float width)
		{
			SpatialTrace.Current.SetLineWidth(width);
		}

		/// <summary>
		/// Reset drawing style to defaults
		/// </summary>
		public static void ResetStyle()
		{
			SpatialTrace.Current.ResetStyle();
		}

		/// <summary>
		/// Indents the trace file for clarity. Remember to call <see cref="Unindent">Unindent</see>.
		/// </summary>
		public static void Indent()
		{
			SpatialTrace.Current.Indent();
		}

		/// <summary>
		/// Unindents the trace file for clarity. Remember to call <see cref="Unindent">Unindent</see>.
		/// </summary>
		public static void Unindent()
		{
			SpatialTrace.Current.Unindent();
		}

		/// <summary>
		/// Global switch to enable tracing.
		/// </summary>
		/// <remarks>Trace is disabled by default. For security, trace will be enabled only if a debugger is attached.</remarks>
		public static void Enable()
		{
			_isEnabled = true;
		}

		/// <summary>
		/// Global switch to disable tracing.
		/// </summary>
		public static void Disable()
		{
			_isEnabled = false;
		}

		/// <summary>
		/// Clears the output file.
		/// </summary>
		public static void Clear()
		{
			SpatialTrace.Current.Clear();
		}

		/// <summary>
		/// Gets the Trace text file path generated
		/// </summary>
		public static string TraceFilePath
		{
			get
			{
				if (_trace != null)
					return _trace.TraceFilePath;
				else
					return SpatialTrace.Current.TraceFilePath;
			}
		}

		/// <summary>
		/// Gets the trace data directory where all geometries are serialized
		/// </summary>
		public static string TraceDataDirectoryName
		{
			get { return TRACE_DATA_DIR; }
		}

		/// <summary>
		/// Gets the trace file title, ie: SpatialTrace.txt
		/// </summary>
		public static string TraceFileName
		{
			get { return TRACE_DATA_FILE; }
		}

		/// <summary>
		/// Displays the trace viewer with the current trace opened
		/// </summary>
		public static void ShowDialog()
		{

		}

	}

	#region Spatial trace implementation

	internal class SpatialTraceInternal : ISpatialTrace
	{
		private bool _isInitialized;
		private StreamWriter _writer;
		private BinaryFormatter _formatter;
		private int _identCount;
		private int _geomIndex;
		private string _outputDirectory;
		private string _outputDirectoryTitle;
		private string _traceFilePath;
		private Color _fillColor = Color.FromArgb(200, 0, 175, 0);
		private Color _strokeColor = Color.FromArgb(255, 0, 0, 0);
		private float _strokeWidth = 1f;

		private string TRACE_LINE_PATTERN = String.Join("\t", new string[] { "{datetime}", "{message}", "{indent}", "{geomfile}", "{callermember}", "{callerfile}", "{callerline}", "{fillcolor}", "{strokecolor}", "{strokewidth}" });


		public SpatialTraceInternal()
		{
			_isInitialized = false;
			SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

			if (System.Diagnostics.Debugger.IsAttached)
			{
				// You are debugging
				Init();
			}


		}

		internal void Init()
		{
			_formatter = new BinaryFormatter();
			_identCount = 0;
			_outputDirectoryTitle = SpatialTrace.TraceDataDirectoryName;
			_outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _outputDirectoryTitle);
			_traceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SpatialTrace.TraceFileName);
			ResetStyle();

			if (_writer != null)
			{
				_writer.Dispose();
			}
			if (Directory.Exists(_outputDirectory))
			{
				Directory.Delete(_outputDirectory, true);
			}
			Directory.CreateDirectory(_outputDirectory);

			_writer = new StreamWriter(_traceFilePath, false);
			_writer.AutoFlush = true;

			WriterHeader();

			_isInitialized = true;
		}

		#region Write line helpers

		private void WriterHeader()
		{
			WriteLine_Internal("DateTime", "Message", "Indent", "GeometryDataFile", "CallerMemberName", "CallerFilePath", "CallerLineNumber", "FillColor", "StrokeColor", "StrokeWidth");
		}
		private void WriteLine(string message, string geomFile, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			WriteLine_Internal(DateTime.Now.ToString("o"), message, _identCount.ToString(), geomFile, memberName, sourceFilePath, sourceLineNumber.ToString(), _fillColor.ToString(), _strokeColor.ToString(), _strokeWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}
		private void WriteLine_Internal(string datetime, string message, string indent, string geomFile, string memberName, string sourceFilePath, string sourceLineNumber, string fillColor, string strokeColor, string strokeWidth)
		{
			string line = TRACE_LINE_PATTERN.Replace("{datetime}", datetime)
																			.Replace("{message}", message.Replace("\t", " "))
																			.Replace("{indent}", indent)
																			.Replace("{geomfile}", geomFile)
																			.Replace("{callermember}", memberName)
																			.Replace("{callerfile}", sourceFilePath)
																			.Replace("{callerline}", sourceLineNumber)
																			.Replace("{fillcolor}", fillColor)
																			.Replace("{strokecolor}", strokeColor)
																			.Replace("{strokewidth}", strokeWidth);
			_writer.WriteLine(line);
		}

		#endregion

		public void TraceGeometry(SqlGeometry geom, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;

			if (geom == null)
			{
				TraceText(message, memberName, sourceFilePath, sourceLineNumber);
			}
			else
			{
				string fileTitle = string.Format("{0}.dat", ++_geomIndex);
				Directory.CreateDirectory(_outputDirectory);
				string fileName = Path.Combine(_outputDirectory, fileTitle);
				using (FileStream dataFile = new FileStream(fileName, FileMode.CreateNew))
				{
					_formatter.Serialize(dataFile, geom);
				}

				WriteLine(message, string.Format("{0}\\{1}", _outputDirectoryTitle, fileTitle), memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public void TraceGeometry(IEnumerable<SqlGeometry> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;
			if (geom == null)
			{
				TraceText(message, memberName, sourceFilePath, sourceLineNumber);
			}
			else
			{
				string fileTitle = string.Format("{0}.list.dat", ++_geomIndex);
				Directory.CreateDirectory(_outputDirectory);
				string fileName = Path.Combine(_outputDirectory, fileTitle);
				using (FileStream dataFile = new FileStream(fileName, FileMode.CreateNew))
				{
					_formatter.Serialize(dataFile, geom.ToList());
				}

				WriteLine(message, string.Format("{0}\\{1}", _outputDirectoryTitle, fileTitle), memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public void TraceGeometry(SqlGeography geog, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;
			SqlGeometry geom = null;
			if (geog.TryToGeometry(out geom))
			{
				TraceGeometry(geom, message, memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public void TraceGeometry(IEnumerable<SqlGeography> geogList, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;
			List<SqlGeometry> geomList = new List<SqlGeometry>();
			foreach (SqlGeography geog in geogList)
			{
				SqlGeometry geom = null;
				if (geog.TryToGeometry(out geom))
				{
					geomList.Add(geom);
				}
			}
			TraceGeometry(geomList, message, memberName, sourceFilePath, sourceLineNumber);
		}

		public void TraceText(string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;
			WriteLine(message, string.Empty, memberName, sourceFilePath, sourceLineNumber);
		}

		public void SetFillColor(Color color)
		{
			if (!_isInitialized) return;
			_fillColor = Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public void SetLineColor(Color color)
		{
			if (!_isInitialized) return;
			_strokeColor = Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public void SetLineWidth(float width)
		{
			if (!_isInitialized) return;
			_strokeWidth = width;
		}

		public void ResetStyle()
		{
			_fillColor = Color.FromArgb(200, 0, 175, 0);
			_strokeColor = Color.FromArgb(255, 0, 0, 0);
			_strokeWidth = 1f;
		}

		public void Indent()
		{
			if (!_isInitialized) return;
			_identCount++;
		}

		public void Unindent()
		{
			if (!_isInitialized) return;
			if (_identCount == 0)
				return;

			_identCount--;
		}

		public void Dispose()
		{
			if (!_isInitialized) return;
			try
			{
				if (_writer != null)
				{
					_writer.Dispose();
					_writer = null;
				}
			}
			catch (Exception)
			{

			}
		}

		public string TraceFilePath
		{
			get { return _traceFilePath; }
		}

		public void Clear()
		{
			try
			{
				Init();
			}
			catch (Exception)
			{
				throw;
			}
		}

	}

	#endregion

	#region Dummy Spatial Trace
	internal class DummySpatialTrace : ISpatialTrace
	{

		public void Indent()
		{

		}

		public void TraceGeometry(SqlGeometry geom, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void TraceGeometry(IEnumerable<SqlGeometry> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber)
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
		public void TraceGeometry(SqlGeography geom, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{

		}

		public void TraceGeometry(IEnumerable<SqlGeography> geom, string message, string memberName, string sourceFilePath, int sourceLineNumber)
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
	#endregion
}
