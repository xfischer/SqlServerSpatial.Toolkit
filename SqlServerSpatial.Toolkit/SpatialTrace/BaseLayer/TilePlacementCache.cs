using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Diagnostics.BaseLayer
{
	public class TilePlacementCache
	{

		Dictionary<int, TilePlacement> _Xplacements = new Dictionary<int, TilePlacement>();
		Dictionary<int, TilePlacement> _Yplacements = new Dictionary<int, TilePlacement>();
		
		private void RegisterPlacement(int x, int y, int posX, int posY, int width, int height)
		{
			TilePlacement placement = new TilePlacement(x, y, posX, posY, width, height);
			RegisterPlacement(placement);
		}
		private void RegisterPlacement(TilePlacement placement)
		{
			if (!_Xplacements.ContainsKey(placement.X))
			{
				_Xplacements[placement.X] = placement;
			}
			if (!_Yplacements.ContainsKey(placement.Y))
			{
				_Yplacements[placement.Y] = placement;
			}
		}
		/// <summary>
		/// But : ne pas autoriser de chevauchements, ne pas autoriser de trous
		/// Implem : pour X : recherche en X-1  pour chech si PosX = posX(x-1) + width(x-1)
		/// pour Y : recherche en Y-1....
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public TilePlacement Suggest(int x, int y, int posX, int posY, int width, int height)
		{
			TilePlacement placement = new TilePlacement(x, y, posX, posY, width, height);

			if (_Xplacements.ContainsKey(x))
			{
				TilePlacement cached = _Xplacements[x];
				placement.PosX = cached.PosX;
				placement.Width = cached.Width;
			}
			else if (_Xplacements.ContainsKey(x - 1))
			{
				int supposedPosX = _Xplacements[x - 1].PosX + _Xplacements[x - 1].Width;
				if (posX != supposedPosX)
				{
					placement.PosX = supposedPosX;
				}
			}

			if (_Yplacements.ContainsKey(y))
			{
				TilePlacement cached = _Yplacements[y];
				placement.PosY = cached.PosY;
				placement.Height = cached.Height;
			}
			else if (_Yplacements.ContainsKey(y - 1))
			{
				int supposedPosY = _Yplacements[y - 1].PosY + _Yplacements[y - 1].Height;
				if (posY != supposedPosY)
				{
					placement.PosY = supposedPosY;
				}
			}

			RegisterPlacement(placement);

			return placement;
		}
	}
	public struct TilePlacement
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int PosX { get; set; }
		public int PosY { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public TilePlacement(int x, int y, int posX, int posY, int width, int height)
			: this()
		{
			X = x;
			Y = y;
			PosX = posX;
			PosY = posY;
			Width = width;
			Height = height;
		}
	}
}
