using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Diagnostics.BaseLayer
{
	public class GeoBitmap : IDisposable
	{
		public Bitmap Bitmap { get; set; }
		public TileIndex Index { get; set; }
		public BoundingBox BBox { get; set; }
		public string OriginUri { get; set; }
		public TileOrigin Origin { get; set; }
		public Exception Exception { get; set; }


		public void Dispose()
		{
			if (Bitmap != null)
			{
				Bitmap.Dispose();
			}
		}
	}

	public enum TileOrigin
	{
		Memory,
		Download,
		Disk
	}

	public class TileIndex : IEquatable<TileIndex>
	{
		private readonly int _x;
		public int X { get { return _x; } }
		private readonly int _y;
		public int Y { get { return _y; } }
		private readonly int _z;
		public int Z { get { return _z; } }

		public TileIndex(int x, int y, int z)
		{
			_x = x; _y = y; _z = z;
			_hash = _x ^ _y ^ _z;
		}

		#region IEquatable<GeoBitmap> Membres

		public bool Equals(TileIndex other)
		{
			if (other == null)
			{
				return false;
			}

			return (X == other.X) && (Y == other.Y) && (Z == other.Z);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			TileIndex tile = obj as TileIndex;
			if (tile == null)
			{
				return false;
			}

			return (X == tile.X) && (Y == tile.Y) && (Z == tile.Z);

		}

		private readonly int _hash;
		public override int GetHashCode()
		{
			return _hash;
		}

		#endregion

		public override string ToString()
		{
			return string.Format("X: {0}, Y: {1}, Z: {2}", X, Y, Z);
		}
	}
}
