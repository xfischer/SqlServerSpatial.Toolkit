using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Diagnostics.BaseLayer
{
	public class ZXYBaseLayer : IBaseLayer
	{
		private readonly string _baseLayerFormatString;
		private readonly string[] _cycle = new string[] { "a", "b", "c" };
		private static int _cycleIndex = 0;
		private readonly bool _stopDownloadBatchIfException;
		private readonly bool _useLowResTiles;

		public ZXYBaseLayer(string UrlFormat, string Name, bool StopBatchIfException, bool useLowResTiles)
		{
			_baseLayerFormatString = UrlFormat;
			_name = Name ?? "ZXYBaseLayer";
			_name += useLowResTiles ? "" : " (HiDef)";
			_stopDownloadBatchIfException = StopBatchIfException;
			_useLowResTiles = useLowResTiles;
		}
		public string GetTileUrl(int zoom, int x, int y)
		{
			string url = _baseLayerFormatString.Replace("{z}", zoom.ToString())
																				.Replace("{x}", x.ToString())
																				.Replace("{y}", y.ToString())
																				.Replace("{c}", _cycle[_cycleIndex++ % 3]);
			return url;
		}

		public int SRID
		{
			get { return 4326; }
		}

		private readonly string _name;
		public string Name
		{
			get { return _name; }
		}


		public bool StopDownloadBatchIfException
		{
			get { return _stopDownloadBatchIfException; }
		}

		public bool UseLowResTiles
		{
			get
			{
				return _useLowResTiles;
			}
		}

	}
}
