using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit.Viewers
{
	/// <summary>
	/// Creates a geometry wrapper class with styles and label
	/// </summary>
	public static class SqlGeomStyledFactory
	{
		private static Color DefaultFillColor = Color.FromArgb(128, 0, 175, 0);
		private static Color DefaultStrokeColor = Colors.Black;
		private static float DefaultStrokeWidth = 1f;

		/// <summary>
		/// Creates a styled geometry
		/// </summary>
		/// <param name="geom">Native SqlGeometry</param>
		/// <param name="label">Label to trace</param>
		/// <param name="fillColor"></param>
		/// <param name="strokeColor"></param>
		/// <param name="strokeWidth"></param>
		/// <returns></returns>
		public static SqlGeometryStyled Create(SqlGeometry geom, string label, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null)
		{
			return new SqlGeometryStyled(geom, label, fillColor ?? DefaultFillColor, strokeColor ?? DefaultStrokeColor, strokeWidth ?? DefaultStrokeWidth);
		}

		/// <summary>
		/// Creates a list of styled geometries
		/// </summary>
		/// <param name="geomList">Native SqlGeometry list</param>
		/// <param name="label"></param>
		/// <param name="fillColor"></param>
		/// <param name="strokeColor"></param>
		/// <param name="strokeWidth"></param>
		/// <returns></returns>
		public static List<SqlGeometryStyled> Create(IEnumerable<SqlGeometry> geomList, string label, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null)
		{
			var list = geomList.Select(g => SqlGeomStyledFactory.Create(g, label, fillColor, strokeColor, strokeWidth)).ToList();
			return list;
		}

		/// <summary>
		/// Creates a styled geography
		/// </summary>
		/// <param name="geom"></param>
		/// <param name="label"></param>
		/// <param name="fillColor"></param>
		/// <param name="strokeColor"></param>
		/// <param name="strokeWidth"></param>
		/// <returns></returns>
		public static SqlGeographyStyled Create(SqlGeography geom, string label, Color? fillColor = null, Color? strokeColor = null, float? strokeWidth = null)
		{
			return new SqlGeographyStyled(geom, label, fillColor ?? DefaultFillColor, strokeColor ?? DefaultStrokeColor, strokeWidth ?? DefaultStrokeWidth);
		}
	}

	/// <summary>
	/// Wrapper around SqlGeometry with style attributes
	/// </summary>
	public class SqlGeometryStyled
	{
		/// <summary>
		/// Native geometry
		/// </summary>
		public SqlGeometry Geometry { get; set; }
		/// <summary>
		/// Associated style
		/// </summary>
		public GeometryStyle Style { get; set; }

		internal SqlGeometryStyled(SqlGeometry geom, string label, Color fillColor, Color strokeColor, float strokeWidth)
		{
			Geometry = geom;
			Style = new GeometryStyle(fillColor, strokeColor, strokeWidth, label);
		}
	}
	/// <summary>
	/// Wrapper around SqlGeography with style attribute
	/// </summary>
	public class SqlGeographyStyled
	{
		/// <summary>
		/// Native geometry
		/// </summary>
		public SqlGeography Geometry { get; set; }
		/// <summary>
		/// Associated style
		/// </summary>
		public GeometryStyle Style { get; set; }

		internal SqlGeographyStyled(SqlGeography geom, string label, Color fillColor, Color strokeColor, float strokeWidth)
		{
			Geometry = geom;
			Style = new GeometryStyle(fillColor, strokeColor, strokeWidth, label);
		}
	}
	/// <summary>
	/// Geometry style
	/// </summary>
	public class GeometryStyle : IEquatable<GeometryStyle>
	{
		/// <summary>
		/// Fill color
		/// </summary>
		public Color FillColor { get; set; }
		/// <summary>
		/// Stroke color
		/// </summary>
		public Color StrokeColor { get; set; }
		/// <summary>
		/// Stroke thickness
		/// </summary>
		public float StrokeWidth { get; set; }
		/// <summary>
		/// Label
		/// </summary>
		public string Label { get; set; }

		internal GeometryStyle(Color fillColor, Color strokeColor, float strokeWidth, string label)
		{
			FillColor = fillColor;
			StrokeColor = strokeColor;
			StrokeWidth = strokeWidth;
			Label = label;
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
				hash = hash + 23 * FillColor.ToString().GetHashCode();
				hash = hash + 23 * StrokeColor.ToString().GetHashCode();
				hash = hash + 23 * StrokeWidth.ToString().GetHashCode();
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
