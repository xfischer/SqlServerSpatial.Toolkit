using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;


namespace SqlServerSpatial.Toolkit
{
	public struct BoundingBox
	{
		private double _xMin;
		public double XMin
		{
			get { return _xMin; }
			set { _xMin = value; }
		}

		private double _xMax;
		public double XMax
		{
			get { return _xMax; }
			set { _xMax = value; }
		}

		private double _yMin;
		public double YMin
		{
			get { return _yMin; }
			set { _yMin = value; }
		}

		private double _yMax;
		public double YMax
		{
			get { return _yMax; }
			set { _yMax = value; }
		}

		public double Width
		{
			get
			{
				return _xMax - _xMin;
			}
		}

		public double Height
		{
			get
			{
				return _yMax - _yMin;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return _xMin == 0
				&& _xMax == 0
				&& _yMax == 0
				&& _yMin == 0;
			}
		}

		public BoundingBox(double xmin, double xmax, double ymin, double ymax)
		{
			_xMin = xmin;
			_xMax = xmax;
			_yMin = ymin;
			_yMax = ymax;
		}

		public BoundingBox Scale(double scale)
		{
			return new BoundingBox(XMin * scale, XMax * scale, YMin * scale, YMax * scale);
		}

		public BoundingBox Transform(System.Drawing.Drawing2D.Matrix matrix)
		{
			System.Drawing.PointF[] points = new System.Drawing.PointF[2];
			points[0] = new System.Drawing.PointF((float)XMin, (float)YMin);
			points[1] = new System.Drawing.PointF((float)XMax, (float)YMax);
			matrix.TransformPoints(points);
			return new BoundingBox(points[0].X, points[1].X, points[0].Y, points[1].Y);
		}

		public override string ToString()
		{
			return string.Format("Xmin: {0} Xmax: {1} Ymin: {2}, Ymax: {3} (Width: {4} Height: {5})", XMin, XMax, YMin, YMax, Width, Height);
		}
	}

	public static class BoundingBoxExtensions
	{
		public static BoundingBox Transform(this Matrix matrix, BoundingBox bbox)
		{
			PointF minPoint = new PointF((float)bbox.XMin, (float)bbox.YMin);
			PointF maxPoint = new PointF((float)bbox.XMax, (float)bbox.YMax);
			var points = new PointF[] { minPoint, maxPoint };
			matrix.TransformPoints(points);

			minPoint = points.First();
			maxPoint = points.Last();

			return new BoundingBox(minPoint.X, maxPoint.X, minPoint.Y, maxPoint.Y);
		}
	}
}
