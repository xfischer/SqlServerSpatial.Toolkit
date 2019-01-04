using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.Diagnostics.Viewers;
using NetTopologySuite.Diagnostics.BaseLayer;

namespace NetTopologySuite.Diagnostics
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
