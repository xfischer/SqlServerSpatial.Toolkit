using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.SqlServer.Types;
using System.Diagnostics;

namespace SqlServerSpatial.Toolkit.Viewers
{
	public static class SqlGeomStyledFactory
	{
		private static Color DefaultFillColor = Color.FromArgb(128, 0, 175, 0);
		private static Color DefaultStrokeColor = Colors.Black;
		private static float DefaultStrokeWidth = 1f;
		
		public static SqlGeometryStyled Create(SqlGeometry geom, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null, string label = null, bool isInDefaultView = false)
		{
			return new SqlGeometryStyled(geom, fillColor ?? DefaultFillColor, strokeColor ?? DefaultStrokeColor, strokeWidth ?? DefaultStrokeWidth, label, isInDefaultView);
		}

		public static List<SqlGeometryStyled> Create(IEnumerable<SqlGeometry> geomList, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null, string label = null, bool isInDefaultView = false)
		{
			var list = geomList.Select(g => SqlGeomStyledFactory.Create(g, fillColor, strokeColor, strokeWidth, label, isInDefaultView)).ToList();
			return list;
		}

		public static SqlGeographyStyled Create(SqlGeography geom, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null, string label = null, bool isInDefaultView = false)
		{
			return new SqlGeographyStyled(geom, fillColor ?? DefaultFillColor, strokeColor ?? DefaultStrokeColor, strokeWidth ?? DefaultStrokeWidth, label, isInDefaultView);
		}
	}
	public class SqlGeometryStyled
	{
		public SqlGeometry Geometry { get; set; }
		public GeometryStyle Style { get; set; }
		public string Label { get; set; }

		public SqlGeometryStyled(SqlGeometry geom, Color fillColor, Color strokeColor, float strokeWidth, string label, bool isInDefaultView)
		{
			Geometry = geom;
			Label = label;
			Style = new GeometryStyle(fillColor, strokeColor, strokeWidth, isInDefaultView);
		}
	}
	public class SqlGeographyStyled
	{
		public SqlGeography Geometry { get; set; }
		public GeometryStyle Style { get; set; }
		public string Label { get; set; }
		public SqlGeographyStyled(SqlGeography geom, Color fillColor, Color strokeColor, float strokeWidth, string label, bool isInDefaultView)
		{
			Geometry = geom;
			Label = label;
			Style = new GeometryStyle(fillColor, strokeColor, strokeWidth, isInDefaultView);
		}
	}
	public class GeometryStyle : IEquatable<GeometryStyle>
	{
		public Color FillColor { get; set; }
		public Color StrokeColor { get; set; }
		public float StrokeWidth { get; set; }
		public bool IsInDefaultView { get; set; }

		public GeometryStyle(Color fillColor, Color strokeColor, float strokeWidth, bool isInDefaultView)
		{
			FillColor = fillColor;
			StrokeColor = strokeColor;
			StrokeWidth = strokeWidth;
			IsInDefaultView = isInDefaultView;
		}


		#region IEquatable<GeometryStyle> Membres

		public override string ToString()
		{
			return string.Concat("Hash: ", this.GetHashCode().ToString());
		}

		public override bool Equals(object obj)
		{
			GeometryStyle objTyped = obj as GeometryStyle;

			return Equals(objTyped);
		}

		public override int GetHashCode()
		{
			return string.Concat(FillColor.ToString(), StrokeColor.ToString(), StrokeWidth.GetHashCode()).GetHashCode();
			//return ComputeStringHash(FillColor, StrokeColor, StrokeWidth);
			//unchecked
			//{
			//	int hash = FillColor.ToString().GetHashCode();
			//	// Maybe nullity checks, if these are objects not primitives!
			//	hash = hash * 29 + StrokeColor.ToString().GetHashCode();
			//	hash = hash * 29 + StrokeWidth.ToString().GetHashCode();
			//	return hash;
			//}
		}

		private int ComputeStringHash(params object[] p_values)
		{
			int v_returned = p_values.Aggregate(0, (p_current, p_value) => (p_current * 397) ^ p_value.GetHashCode());
			return v_returned;
		}

		public bool Equals(GeometryStyle other)
		{
			if (other == null)
				return false;

			return this.GetHashCode().Equals(other.GetHashCode());
		}

		#endregion
	}
}
