using System.Collections.Generic;
using NetTopologySuite.Diagnostics.BaseLayer;

namespace NetTopologySuite.Diagnostics.Viewers
{
	public interface IMapViewModel
	{
		IBaseLayer BaseLayer { get; set; }
		List<IBaseLayer> BaseLayers { get; }
	}
}