using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Diagnostics.Viewers.GDI
{
	public class ViewerInfoEventArgs
	{
		public ViewerInfoType InfoType { get; internal set; }
		public string GeometryInfo { get; internal set; }
		public long InitTime { get; internal set; }
		public long DrawTime { get; internal set; }
	}

	[Flags]
	public enum ViewerInfoType
	{
		None											= 0,
		InitDone									= 1,
		Draw											= 1 << 1,
		MouseMove									= 1 << 2,
	}

}
