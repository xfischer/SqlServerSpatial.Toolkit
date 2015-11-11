using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit.Viewers
{
	/// <summary>
	/// Sinks that fills two graphics paths : one for the filled geometries and one other for the outlines
	/// </summary>
	internal class SqlGeometryGDISink : IGeometrySink110
	{
		GraphicsPath _gpStroke;
		GraphicsPath _gpFill;
		List<PointF> _currentLine;
		Vector _unitVector;

		public static void ConvertSqlGeometry(SqlGeometry geom, Vector unitVector, ref GraphicsPath stroke, ref GraphicsPath fill)
		{
			SqlGeometryGDISink sink = new SqlGeometryGDISink(stroke, fill, unitVector);
			geom.Populate(sink);
		}

		private SqlGeometryGDISink(GraphicsPath gpStroke, GraphicsPath gpFill, Vector unitVector)
		{
			_gpStroke = gpStroke;
			_gpFill = gpFill;
			_currentLine = new List<PointF>();
			_unitVector = unitVector;
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (_curType == OpenGisGeometryType.Point)
			{
				// for a point, we draw an ellipse with the provider unit vector				
				_gpFill.AddEllipse((float)x, (float)y, (float)(_unitVector.Length * 2d), (float)(_unitVector.Length * 2d));
				_gpStroke.AddEllipse((float)x, (float)y, (float)(_unitVector.Length * 2d), (float)(_unitVector.Length * 2d));
			}
			else
			{
				_gpStroke.StartFigure();

				_currentLine.Clear();
				_currentLine.Add(new PointF((float)x, (float)y));
			}
		}
		public void AddLine(double x, double y, double? z, double? m)
		{
			_currentLine.Add(new PointF((float)x, (float)y));
		}
		public void EndFigure()
		{
			if (_curType != OpenGisGeometryType.Point)
			{
				_gpStroke.CloseFigure();

				PointF[] coords = _currentLine.ToArray();
				_gpStroke.AddLines(coords);

				if (_curType == OpenGisGeometryType.Polygon)
				{
					_gpFill.AddPolygon(coords);
				}
			}
		}

		OpenGisGeometryType _curType;
		public void BeginGeometry(OpenGisGeometryType type)
		{
			_curType = type;
		}

		public void EndGeometry()
		{
		}

		public void SetSrid(int srid)
		{

		}


		public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
		{
			throw new NotImplementedException();
		}
	}
}
