using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerSpatial.Toolkit.BaseLayer
{
	public interface IBaseLayer
	{
		string Name { get; }
		string GetTileUrl(int zoom, int x, int y);
		int SRID { get; }
		bool StopDownloadBatchIfException { get; }

		bool UseLowResTiles { get; }
	}
}
