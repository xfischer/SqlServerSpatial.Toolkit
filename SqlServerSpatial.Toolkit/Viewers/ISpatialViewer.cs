using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlServerSpatial.Toolkit.Viewers;
using SqlServerSpatial.Toolkit.BaseLayer;

namespace SqlServerSpatial.Toolkit
{
	public interface ISpatialViewer
	{
		void SetGeometry(SqlGeometryStyled geometry);
		void SetGeometry(IEnumerable<SqlGeometryStyled> geometries);
		void SetGeometry(SqlGeographyStyled geography);
		void SetGeometry(IEnumerable<SqlGeographyStyled> geographies);
		void Clear();

		void ResetView();

		string GetSQLSourceText();

	}

	
}
