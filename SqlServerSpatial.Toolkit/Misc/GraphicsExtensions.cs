using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Diagnostics
{

	internal static class Graphicsxtensions
	{
		/// <summary>
		/// Conversion from GDI color to WPF color
		/// </summary>
		/// <param name="Color"></param>
		/// <returns></returns>
		public static System.Windows.Media.Color ToWpf(this System.Drawing.Color Color)
		{
			return System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, Color.B);
		}

		/// <summary>
		/// Conversion from WPF color to GDI color
		/// </summary>
		/// <param name="Color"></param>
		/// <returns></returns>
		public static System.Drawing.Color ToGDI(this System.Windows.Media.Color Color)
		{
			return System.Drawing.Color.FromArgb(Color.A, Color.R, Color.G, Color.B);
		}



		/// <summary>  
		/// method for changing the opacity of an image  
		/// </summary>  
		/// <param name="image">image to set opacity on</param>  
		/// <param name="opacity">percentage of opacity</param>  
		/// <returns></returns>  
		public static Image SetImageOpacity(this Image image, float opacity)
		{
			try
			{
				//create a Bitmap the size of the image provided  
				Bitmap bmp = new Bitmap(image.Width, image.Height);

				//create a graphics object from the image  
				using (Graphics gfx = Graphics.FromImage(bmp))
				{

					//create a color matrix object  
					ColorMatrix matrix = new ColorMatrix();

					//set the opacity  
					matrix.Matrix33 = opacity;

					//create image attributes  
					ImageAttributes attributes = new ImageAttributes();

					//set the color(opacity) of the image  
					attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

					//now draw the image  
					gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
				}
				return bmp;
			}
			catch (Exception ex)
			{
				Trace.TraceError("SetImageOpacity: " + ex.Message);
				return null;
			}
		}
	}
}
