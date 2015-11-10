using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SqlServerSpatial.Toolkit
{
	internal class TraceLineDesign
	{
		public DateTime DateTime { get; set; }
		public string DateTimeDesign { get; private set; }
		public string Message { get; set; }
		public int Indent { get; set; }
		public Thickness IndentMargin
		{
			get
			{
				return new Thickness(Indent * 20, 0, 0, 0);
			}

		}
		public string GeometryDataFile { get; set; }		
		public string CallerMemberName { get; set; }
		public string CallerFilePath { get; set; }
		public int CallerLineNumber { get; set; }
		public Color FillColor { get; set; }
		public Color StrokeColor { get; set; }
		public float StrokeWidth { get; set; }
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

		private TraceLineDesign()
		{
		}

		public static TraceLineDesign Parse(string lineText)
		{
			try
			{
				string[] lineParts = lineText.Split('\t');
				TraceLineDesign currentLine = new TraceLineDesign();
				int i = 0;
				currentLine.DateTime = DateTime.Parse(lineParts[i]);
				i++;
				currentLine.Message = lineParts[i];
				i++;
				currentLine.Indent= int.Parse(lineParts[i]);
				i++;
				currentLine.GeometryDataFile = lineParts[i];
				i++;
				currentLine.CallerMemberName = lineParts[i];
				i++;
				currentLine.CallerFilePath = lineParts[i];
				i++;
				currentLine.CallerLineNumber = int.Parse(lineParts[i]);
				i++;
				if (i < lineParts.Length) currentLine.FillColor = (Color)ColorConverter.ConvertFromString(lineParts[i]);
				i++;
				if (i < lineParts.Length) currentLine.StrokeColor = (Color)ColorConverter.ConvertFromString(lineParts[i]);
				i++;
				if (i < lineParts.Length) currentLine.StrokeWidth = float.Parse(lineParts[i], CultureInfo.InvariantCulture);

				// Append millisecond pattern to current culture's full date time pattern 
				currentLine.DateTimeDesign = currentLine.DateTime.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern + " " + DateTimeFormatInfo.CurrentInfo.LongTimePattern + ".ffff");

				return currentLine;
			}
			catch (Exception)
			{
				throw;
			}
		}

		
	}
}
