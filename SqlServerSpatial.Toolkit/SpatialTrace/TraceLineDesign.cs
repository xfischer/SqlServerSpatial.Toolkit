using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NetTopologySuite.Diagnostics
{

	public class TraceLineDesign : NotifyPropertyChangedBase, IEquatable<TraceLineDesign>
	{
		private static int _uniqueIndexSequence = 0;
		private static string _lastGroupName = null;

		public int UniqueId { get; private set; }
		public DateTime DateTime { get; set; }
		public string DateTimeDesign { get; private set; }
		public string Message { get; set; }
		public string Indent { get; set; }
		public string GeometryDataFile { get; set; }
		public string CallerMemberName { get; set; }
		public string CallerFilePath { get; set; }
		public int CallerLineNumber { get; set; }
		public Color FillColor { get; set; }
		public Color StrokeColor { get; set; }
		public float StrokeWidth { get; set; }

		public bool IsGroupHeader { get; set; }
		private bool _isExpanded;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					if (IsGroupHeader)
					{
						NotifyOfPropertyChange(() => IsExpanded);
					}
				}
			}
		}

		public Brush FillBrush
		{
			get { return new SolidColorBrush(FillColor); }
		}
		public Brush Stroke
		{
			get { return new SolidColorBrush(StrokeColor); }
		}
		public double StrokeThickness
		{
			get { return StrokeWidth; }
		}
		public string Label { get; set; }

		private bool _isChecked;
		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					NotifyOfPropertyChange(() => IsChecked);
				}
			}
		}


		private TraceLineDesign()
		{
			this.UniqueId = ++_uniqueIndexSequence;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || this == null)
				return false;

			TraceLineDesign other = obj as TraceLineDesign;
			if (other == null)
				return false;

			return this.UniqueId == other.UniqueId;
		}
		public override int GetHashCode()
		{
			return this.UniqueId;
		}
		public bool Equals(TraceLineDesign other)
		{
			if (other == null || this == null)
				return false;


			return this.UniqueId == other.UniqueId;
		}

		public static TraceLineDesign Parse(string lineText)
		{

			try
			{
				string[] lineParts = lineText.Split('\t');
				bool hasLabel = lineParts.Length == 11;
				TraceLineDesign currentLine = new TraceLineDesign();
				int i = 0;
				currentLine.DateTime = DateTime.Parse(lineParts[i++]);
				currentLine.Message = lineParts[i++];
				currentLine.Label = hasLabel ? lineParts[i++] : currentLine.Message;
				currentLine.Indent = lineParts[i++];
				currentLine.GeometryDataFile = lineParts[i++];
				currentLine.CallerMemberName = lineParts[i++];
				currentLine.CallerFilePath = lineParts[i++];
				currentLine.CallerLineNumber = int.Parse(lineParts[i++]);
				if (i < lineParts.Length) currentLine.FillColor = (Color)ColorConverter.ConvertFromString(lineParts[i++]);
				if (i < lineParts.Length) currentLine.StrokeColor = (Color)ColorConverter.ConvertFromString(lineParts[i++]);
				if (i < lineParts.Length) currentLine.StrokeWidth = float.Parse(lineParts[i++], CultureInfo.InvariantCulture);

				// Append millisecond pattern to current culture's full date time pattern 
				currentLine.DateTimeDesign = currentLine.DateTime.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern + " " + DateTimeFormatInfo.CurrentInfo.LongTimePattern + ".ffff");

				if (currentLine.Indent != _lastGroupName)
				{
					currentLine.IsGroupHeader = true;
					_lastGroupName = currentLine.Indent;
				}

				return currentLine;
			}
			catch (Exception)
			{
				throw;
			}
		}


	}
}
