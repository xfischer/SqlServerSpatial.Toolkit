using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit.Viewers
{
	internal class SqlGeometryGDISink : IGeometrySink110
	{
		GraphicsPath _gpStroke;
		GraphicsPath _gpFill;
		List<PointF> _currentLine;

		private SqlGeometryGDISink(GraphicsPath gpStroke, GraphicsPath gpFill)
		{
			_gpStroke = gpStroke;
			_gpFill = gpFill;
			_currentLine = new List<PointF>();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_gpStroke.StartFigure();

			_currentLine.Clear();
			_currentLine.Add(new PointF((float)x, (float)y));
		}
		public void AddLine(double x, double y, double? z, double? m)
		{
			_currentLine.Add(new PointF((float)x, (float)y));
		}
		public void EndFigure()
		{
			_gpStroke.CloseFigure();

			PointF[] coords = _currentLine.ToArray();
			_gpStroke.AddLines(coords);

			if (_curType == OpenGisGeometryType.Polygon)
			{
				_gpFill.AddPolygon(coords);
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

		public static void ConvertSqlGeometry(SqlGeometry geom, ref GraphicsPath stroke, ref GraphicsPath fill)
		{
			SqlGeometryGDISink sink = new SqlGeometryGDISink(stroke, fill);
			geom.Populate(sink);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new NotImplementedException();
        }
    }
}
