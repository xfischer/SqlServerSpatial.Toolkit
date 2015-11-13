using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServerSpatial.Toolkit
{

	public static class ColorExtensions
	{
		/// <summary>
		/// Conversion from GDI color to WPF color
		/// </summary>
		/// <param name="gdiColor"></param>
		/// <returns></returns>
		public static System.Windows.Media.Color ToWpf(this System.Drawing.Color gdiColor)
		{
			return System.Windows.Media.Color.FromArgb(gdiColor.A, gdiColor.R, gdiColor.G, gdiColor.B);
		}

		/// <summary>
		/// Conversion from WPF color to GDI color
		/// </summary>
		/// <param name="gdiColor"></param>
		/// <returns></returns>
		public static System.Drawing.Color ToGDI(this System.Windows.Media.Color gdiColor)
		{
			return System.Drawing.Color.FromArgb(gdiColor.A, gdiColor.R, gdiColor.G, gdiColor.B);
		}
	}
}
