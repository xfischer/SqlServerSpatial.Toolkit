using GeoAPI.Geometries;
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
		void SetGeometry(IGeometryStyled geometry);
		void SetGeometry(IEnumerable<IGeometryStyled> geometries);
		void Clear();

		void ResetView();

		string GetSQLSourceText();

	}

	
}
