using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerSpatial.Toolkit.BaseLayer
{
	public interface IBaseLayerViewer
	{
		void SetBaseLayer(IBaseLayer BaseLayer);
		
		bool Enabled { get; set; }

		float Opacity { get; set; }

		bool ShowLabels { get; set; }
	}
}
