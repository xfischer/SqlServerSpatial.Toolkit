using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlServerSpatialTypes.Toolkit.Viewers;

namespace SqlServerSpatialTypes.Toolkit
{
	public interface ISpatialViewer
	{
		void SetGeometry(SqlGeometryStyled geometry);
		void SetGeometry(IEnumerable<SqlGeometryStyled> geometries);
		void SetGeometry(SqlGeographyStyled geography);
		void SetGeometry(IEnumerable<SqlGeographyStyled> geographies);
		void Clear();

		void ResetView();
	}
}
