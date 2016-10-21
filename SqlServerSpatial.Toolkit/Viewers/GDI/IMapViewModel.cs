using System.Collections.Generic;
using SqlServerSpatial.Toolkit.BaseLayer;

namespace SqlServerSpatial.Toolkit.Viewers
{
	public interface IMapViewModel
	{
		IBaseLayer BaseLayer { get; set; }
		List<IBaseLayer> BaseLayers { get; }
	}
}