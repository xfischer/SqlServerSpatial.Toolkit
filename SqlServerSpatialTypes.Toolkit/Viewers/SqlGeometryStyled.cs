using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatialTypes.Toolkit.Viewers
{
	public static class SqlGeomStyledFactory
	{
		private static Color DefaultFillColor = Color.FromArgb(128, 0, 175, 0);
		private static Color DefaultStrokeColor = Colors.Black;
		private static float DefaultStrokeWidth = 1f;

		public static SqlGeometryStyled Create(SqlGeometry geom, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null)
		{
			return new SqlGeometryStyled(geom, fillColor ?? DefaultFillColor, strokeColor ?? DefaultStrokeColor, strokeWidth ?? DefaultStrokeWidth);
		}

		public static List<SqlGeometryStyled> Create(IEnumerable<SqlGeometry> geomList, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null)
		{
			var list = geomList.Select(g => SqlGeomStyledFactory.Create(g, fillColor, strokeColor, strokeWidth)).ToList();
			return list;
		}

		public static SqlGeographyStyled Create(SqlGeography geom, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null)
		{
			return new SqlGeographyStyled(geom, fillColor ?? DefaultFillColor, strokeColor ?? DefaultStrokeColor, strokeWidth ?? DefaultStrokeWidth);
		}
	}
	public class SqlGeometryStyled
	{
		public SqlGeometry Geometry { get; set; }
		public GeometryStyle Style { get; set; }

		public SqlGeometryStyled(SqlGeometry geom, Color fillColor, Color strokeColor, float strokeWidth)
		{
			Geometry = geom;
			Style = new GeometryStyle(fillColor, strokeColor, strokeWidth);
		}
	}
	public class SqlGeographyStyled
	{
		public SqlGeography Geometry { get; set; }
		public GeometryStyle Style { get; set; }

		public SqlGeographyStyled(SqlGeography geom, Color fillColor, Color strokeColor, float strokeWidth)
		{
			Geometry = geom;
			Style = new GeometryStyle(fillColor, strokeColor, strokeWidth);
		}
	}
	public class GeometryStyle : IEquatable<GeometryStyle>
	{
		public Color FillColor { get; set; }
		public Color StrokeColor { get; set; }
		public float StrokeWidth { get; set; }

		public GeometryStyle(Color fillColor, Color strokeColor, float strokeWidth)
		{
			FillColor = fillColor;
			StrokeColor = strokeColor;
			StrokeWidth = strokeWidth;
		}


		#region IEquatable<GeometryStyle> Membres

		public override bool Equals(object obj)
		{
			GeometryStyle objTyped = obj as GeometryStyle;

			return Equals(objTyped);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				// Maybe nullity checks, if these are objects not primitives!
				hash = hash * 23 + FillColor.GetHashCode();
				hash = hash * 23 + StrokeColor.GetHashCode();
				hash = hash * 23 + StrokeWidth.GetHashCode();
				return hash;
			}
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
