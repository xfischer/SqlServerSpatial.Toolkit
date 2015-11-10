using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SqlServerSpatial.Toolkit.Viewers
{
	internal class MouseWheelMessageFilter : IMessageFilter
	{

		const int WM_MOUSEWHEEL = 0x20a;

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == WM_MOUSEWHEEL)
			{
				// LParam contains the location of the mouse pointer
				Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
				IntPtr hWnd = WindowFromPoint(pos);
				if (hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null)
				{
					// redirect the message to the correct control
					SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
					return true;
				}
			}
			return false;
		}

		// P/Invoke declarations
		[DllImport("user32.dll")]
		private static extern IntPtr WindowFromPoint(Point pt);
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
	}
}
