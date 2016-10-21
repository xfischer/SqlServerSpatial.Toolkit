using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;

namespace SqlServerSpatial.Toolkit
{
	internal class SpatialTraceInternal : ISpatialTrace
	{
		private bool _isInitialized;
		private StreamWriter _writer;
		private BinaryFormatter _formatter;
		private int _identCount;
		private string _identGroupName;
		private int _geomIndex;
		private string _outputBaseDirectory;
		private string _outputDirectory;
		private string _outputDirectoryTitle;
		private string _traceFilePath;
		private Color _fillColor = Color.FromArgb(200, 0, 175, 0);
		private Color _strokeColor = Color.FromArgb(255, 0, 0, 0);
		private float _strokeWidth = 1f;

		private string TRACE_LINE_PATTERN = String.Join("\t", new string[] { "{datetime}", "{message}", "{label}", "{indent}", "{geomfile}", "{callermember}", "{callerfile}", "{callerline}", "{fillcolor}", "{strokecolor}", "{strokewidth}" });


		public SpatialTraceInternal(string outputBaseDirectory)
		{
			_isInitialized = false;
			_outputBaseDirectory = outputBaseDirectory;
			Init();
		}

		internal void Init()
		{
			_formatter = new BinaryFormatter();
			_identCount = 0;
			_outputDirectoryTitle = SpatialTrace.TraceDataDirectoryName;
			_outputDirectory = Path.Combine(_outputBaseDirectory, _outputDirectoryTitle);
			_traceFilePath = Path.Combine(_outputBaseDirectory, SpatialTrace.TraceFileName);
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
			WriteLine_Internal("DateTime", "Message", "Label", "Indent", "GeometryDataFile", "CallerMemberName", "CallerFilePath", "CallerLineNumber", "FillColor", "StrokeColor", "StrokeWidth");
		}
		private void WriteLine(string message, string label, string geomFile, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			WriteLine_Internal(DateTime.Now.ToString("o"), message, label, _identGroupName, geomFile, memberName, sourceFilePath, sourceLineNumber.ToString(), _fillColor.ToString(), _strokeColor.ToString(), _strokeWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}
		private void WriteLine_Internal(string datetime, string message, string label, string indent, string geomFile, string memberName, string sourceFilePath, string sourceLineNumber, string fillColor, string strokeColor, string strokeWidth)
		{
			string line = TRACE_LINE_PATTERN.Replace("{datetime}", datetime)
																			.Replace("{message}", message.Replace("\t", " "))
																			.Replace("{label}", (label ?? string.Empty).Replace("\t", " "))
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

		public void TraceGeometry(SqlGeometry geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
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

				WriteLine(message, label, string.Format("{0}\\{1}", _outputDirectoryTitle, fileTitle), memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public void TraceGeometry(IEnumerable<SqlGeometry> geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
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

				WriteLine(message, label, string.Format("{0}\\{1}", _outputDirectoryTitle, fileTitle), memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public void TraceGeometry(SqlGeography geog, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;
			SqlGeometry geom = null;
			if (geog.TryToGeometry(out geom))
			{
				TraceGeometry(geom, message, label, memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public void TraceGeometry(IEnumerable<SqlGeography> geogList, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber)
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
			TraceGeometry(geomList, label, message, memberName, sourceFilePath, sourceLineNumber);
		}

		public void TraceText(string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			if (!_isInitialized) return;
			WriteLine(message, string.Empty, string.Empty, memberName, sourceFilePath, sourceLineNumber);
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

		public void Indent(string groupName = null)
		{
			if (!_isInitialized) return;
			_identCount++;
			_identGroupName = groupName ?? _identCount.ToString();
		}

		public void Unindent()
		{
			if (!_isInitialized) return;
			if (_identCount > 0)
			{
				_identCount--;
			}
			if (_identCount == 0)
			{
				_identGroupName = null;
			}
			
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
}
