using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerSpatial.Toolkit.BaseLayer
{
	public class EmptyBaseLayer : IBaseLayer
	{
		public string Name
		{
			get
			{
				return "None";
			}
		}

		public int SRID
		{
			get
			{
				return 0;
			}
		}

		public bool StopDownloadBatchIfException
		{
			get
			{
				return true;
			}
		}

		public bool UseLowResTiles
		{
			get
			{
				return false;
			}
		}

		public string GetTileUrl(int zoom, int x, int y)
		{
			return null;
		}
	}
}
