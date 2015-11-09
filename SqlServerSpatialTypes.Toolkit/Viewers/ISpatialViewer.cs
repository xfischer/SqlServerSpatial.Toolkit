using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlServerSpatialTypes.Toolkit.Viewers;

namespace SqlServerSpatialTypes.Toolkit
{
	/// <summary>
	/// Defines the contract between the viewer control and a spatial visualizer
	/// </summary>
	public interface ISpatialViewer
	{
		/// <summary>
		/// Draws the specified geometry with its style and label to the viewer
		/// </summary>
		/// <param name="geometry"></param>
		void SetGeometry(SqlGeometryStyled geometry);
		/// <summary>
		/// Draws the specified geometries with its style and label to the viewer
		/// </summary>
		/// <param name="geometries"></param>
		void SetGeometry(IEnumerable<SqlGeometryStyled> geometries);
		/// <summary>
		/// Draws the specified geography with its style and label to the viewer
		/// </summary>
		/// <param name="geography"></param>
		void SetGeometry(SqlGeographyStyled geography);
		/// <summary>
		/// Draws the specified geographies with its style and label to the viewer
		/// </summary>
		/// <param name="geographies"></param>
		void SetGeometry(IEnumerable<SqlGeographyStyled> geographies);
		/// <summary>
		/// Clears the visual
		/// </summary>
		void Clear();

		/// <summary>
		/// Reset zoom and pan
		/// </summary>
		void ResetView();
	}
}
