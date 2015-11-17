using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServerSpatial.Toolkit.Viewers
{
	/// <summary>
	/// EventArgs provided when a spatial viewer communicates with the caller
	/// Event type is stored in InfoType property
	/// </summary>
	public class ViewerInfoEventArgs : EventArgs
	{
		/// <summary>
		/// Type of broadcasted information
		/// </summary>
		public ViewerInfoType InfoType { get; internal set; }

		/// <summary>
		/// Geometry information dumped (number of shapes and points)
		/// </summary>
		public string GeometryInfo { get; internal set; }

		/// <summary>
		/// Time taken in initialization
		/// </summary>
		public long InitTime { get; internal set; }

		/// <summary>
		/// Time taken for last draw operation
		/// </summary>
		public long DrawTime { get; internal set; }
	}

	/// <summary>
	/// Type of broadcasted information
	/// </summary>
	[Flags]
	public enum ViewerInfoType
	{
		/// <summary>
		/// Default value. Is never set
		/// </summary>
		None = 0,
		/// <summary>
		/// Initialization done
		/// </summary>
		InitDone = 1,
		/// <summary>
		/// Geometries drawn
		/// </summary>
		Draw = 1 << 1,
		/// <summary>
		/// Mouse moved (not implemented yet)
		/// </summary>
		MouseMove = 1 << 2,
	}
}
