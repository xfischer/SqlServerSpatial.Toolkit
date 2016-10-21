using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SqlServerSpatial.Toolkit.BaseLayer
{
	public class TileDownloader : IDisposable
	{
		private const string USER_AGENT = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";

		private Dictionary<string, Dictionary<TileIndex, Bitmap>> _imageCache = new Dictionary<string, Dictionary<TileIndex, Bitmap>>();
		private Dictionary<string, Dictionary<TileIndex, DateTime>> _imageCacheDates = new Dictionary<string, Dictionary<TileIndex, DateTime>>();
		private Timer _imageCacheTimer;
		private const int CACHE_INTERVAL = 30000; // 30s
		private const int CACHE_LIFETIME_MIN = 21600; // 15 jours
		private object _syncLock = new object();

		private readonly string _rootDir;
		public TileDownloader()
		{
			_rootDir = Path.Combine(Path.GetTempPath(), "SqlServerSpatial.Toolkit");
			_imageCacheTimer = new Timer(CACHE_INTERVAL);
			_imageCacheTimer.Elapsed += _imageCacheTimer_Elapsed;
			_imageCacheTimer.Start();
		}

		bool _isBusy = false;
		void _imageCacheTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (_isBusy)
				return;

			lock (_syncLock)
			{
				_isBusy = true;
				foreach (string baseLayer in _imageCache.Keys)
				{
					List<TileIndex> obsoleteTiles = _imageCacheDates[baseLayer].Where(kvp => kvp.Value < DateTime.Now).Select(kvp => kvp.Key).ToList();
					foreach (TileIndex tileIndex in obsoleteTiles)
					{
						RemoveTileFromCache(tileIndex, baseLayer);
					}
				}
				_isBusy = false;
			}
		}
		private void SaveTileToCache(Bitmap bmp, TileIndex index, string baseLayerName)
		{
			lock (_syncLock)
			{
				if (_imageCache.ContainsKey(baseLayerName) == false)
					_imageCache[baseLayerName] = new Dictionary<TileIndex, Bitmap>();
				if (_imageCacheDates.ContainsKey(baseLayerName) == false)
					_imageCacheDates[baseLayerName] = new Dictionary<TileIndex, DateTime>();

				_imageCache[baseLayerName][index] = bmp;
				_imageCacheDates[baseLayerName][index] = DateTime.Now.AddMinutes(CACHE_LIFETIME_MIN);

			}
		}
		private Bitmap GetTileFromCache(TileIndex index, string baseLayerName)
		{
			lock (_syncLock)
			{
				if (_imageCache.ContainsKey(baseLayerName) == false)
					return null;

				if (_imageCacheDates.ContainsKey(baseLayerName) == false)
					return null;

				if (_imageCache[baseLayerName].ContainsKey(index) == false)
					return null;

				// update life time
				_imageCacheDates[baseLayerName][index] = DateTime.Now.AddMinutes(CACHE_LIFETIME_MIN);
				return _imageCache[baseLayerName][index];

			}
		}
		private void RemoveTileFromCache(TileIndex index, string baseLayerName)
		{
			lock (_syncLock)
			{
				if (_imageCache.ContainsKey(baseLayerName) == false)
					return;
				if (_imageCacheDates.ContainsKey(baseLayerName) == false)
					return;

				_imageCache[baseLayerName][index].Dispose();
				_imageCache[baseLayerName].Remove(index);
				_imageCacheDates[baseLayerName].Remove(index);
			}
		}

		public GeoBitmap DownloadTile(int zoom, int x, int y, IBaseLayer baseLayer)
		{
			Uri uri = new Uri(baseLayer.GetTileUrl(zoom, x, y));

			GeoBitmap geoBmp = new GeoBitmap() { OriginUri = uri.ToString() };

			try
			{

				TileIndex index = new TileIndex(x, y, zoom);
				Bitmap tileImg = null;
				tileImg = GetTileFromCache(index, baseLayer.Name);
				if (tileImg != null)
				{
					geoBmp.Origin = TileOrigin.Memory;
				}
				else
				{
					tileImg = GetTileFromDisc(index, baseLayer.Name);
					if (tileImg != null)
					{
						geoBmp.Origin = TileOrigin.Disk;
						SaveTileToCache(tileImg, index, baseLayer.Name);
					}
				}

				if (tileImg == null)
				{
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
					request.Timeout = 500;
					request.UserAgent = USER_AGENT;
					//IWebProxy webProxy = WebRequest.DefaultWebProxy;
					//webProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
					//request.Proxy = webProxy;

					try
					{
						using (WebResponse response = request.GetResponse())
						using (Stream stream = response.GetResponseStream())
						{
							tileImg = (Bitmap)Bitmap.FromStream(stream);
						}
					}
					catch (WebException webEx)
					{
						Trace.TraceWarning("Unable to download image  at " + uri.ToString() + ": " + webEx.Message);
						tileImg = new Bitmap(256, 256);
						using (Graphics g = Graphics.FromImage(tileImg))
						{
							g.Clear(Color.LightGray);
							g.DrawRectangle(Pens.Red, 0, 0, 255, 255);
							g.DrawLine(Pens.Red, 1, 1, 255, 255);
						}
						geoBmp.Exception = webEx;
					}
					finally
					{
						geoBmp.Origin = TileOrigin.Download;
						if (geoBmp.Exception == null)
						{
							SaveTileToDisc(tileImg, index, baseLayer.Name);
							SaveTileToCache(tileImg, index, baseLayer.Name);
						}
					}
				}
				geoBmp.Bitmap = tileImg;

				// what are X,Y coords for images
				int xPos, yPos = 0;
				double lat, lon = 0;
				double lat2, lon2 = 0;
				BingMapsTileSystem.TileXYToPixelXY(x, y, out xPos, out yPos);
				BingMapsTileSystem.PixelXYToLatLong(xPos, yPos, zoom, out lat, out lon);

				BingMapsTileSystem.PixelXYToLatLong(xPos + 256, yPos + 256, zoom, out lat2, out lon2);

				geoBmp.BBox = new BoundingBox(lon, lon2, lat2, lat);
				geoBmp.Index = new TileIndex(x, y, zoom);


			}
			catch (Exception ex)
			{
				geoBmp.Exception = ex;
				Trace.TraceWarning("Unable to load base layer at " + uri.ToString() + ": " + ex.Message);
			}
			return geoBmp;
		}
		//public async Task<GeoBitmap> DownloadTileAsync(int zoom, int x, int y, IBaseLayer baseLayer)
		//{
		//	Uri uri = new Uri(baseLayer.GetTileUrl(zoom, x, y));

		//	GeoBitmap geoBmp = new GeoBitmap() { OriginUri = uri.ToString() };

		//	try
		//	{

		//		TileIndex index = new TileIndex(x, y, zoom);
		//		Bitmap tileImg = null;
		//		tileImg = GetTileFromCache(index, baseLayer.Name);
		//		if (tileImg != null)
		//		{
		//			geoBmp.Origin = TileOrigin.Memory;
		//		}
		//		else
		//		{
		//			tileImg = GetTileFromDisc(index, baseLayer.Name);
		//			if (tileImg != null)
		//			{
		//				geoBmp.Origin = TileOrigin.Disk;
		//				SaveTileToCache(tileImg, index, baseLayer.Name);
		//			}
		//		}

		//		if (tileImg == null)
		//		{
		//			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
		//			request.UserAgent = USER_AGENT;
		//			//IWebProxy webProxy = WebRequest.DefaultWebProxy;
		//			//webProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
		//			//request.Proxy = webProxy;

		//			try
		//			{
		//				using (WebResponse response = await request.GetResponseAsync())
		//				using (Stream stream = response.GetResponseStream())
		//				{
		//					tileImg = (Bitmap)Bitmap.FromStream(stream);
		//				}
		//			}
		//			catch (WebException webEx)
		//			{
		//				//Trace.TraceWarning("Unable to download image  at " + uri.ToString() + ": " + webEx.Message);
		//				tileImg = new Bitmap(256, 256);
		//				using (Graphics g = Graphics.FromImage(tileImg))
		//				{
		//					g.Clear(Color.LightGray);
		//					g.DrawRectangle(Pens.Red, 0, 0, 255, 255);
		//					g.DrawLine(Pens.Red, 1, 1, 255, 255);
		//				}
		//				geoBmp.Exception = webEx;
		//			}
		//			finally
		//			{
		//				geoBmp.Origin = TileOrigin.Download;
		//				SaveTileToDisc(tileImg, index, baseLayer.Name);
		//				SaveTileToCache(tileImg, index, baseLayer.Name);
		//			}
		//		}
		//		geoBmp.Bitmap = tileImg;

		//		// what are X,Y coords for images
		//		int xPos, yPos = 0;
		//		double lat, lon = 0;
		//		double lat2, lon2 = 0;
		//		BingMapsTileSystem.TileXYToPixelXY(x, y, out xPos, out yPos);
		//		BingMapsTileSystem.PixelXYToLatLong(xPos, yPos, zoom, out lat, out lon);

		//		BingMapsTileSystem.PixelXYToLatLong(xPos + 256, yPos + 256, zoom, out lat2, out lon2);

		//		geoBmp.BBox = new BoundingBox(lon, lon2, lat2, lat);
		//		geoBmp.Index = new TileIndex(x, y, zoom);


		//	}
		//	catch (Exception ex)
		//	{
		//		geoBmp.Exception = ex;
		//		Trace.TraceWarning("Unable to load base layer at " + uri.ToString() + ": " + ex.Message);
		//	}
		//	return geoBmp;
		//}

		private Bitmap GetTileFromDisc(TileIndex index, string baseLayerName)
		{
			try
			{
				string path = GetTilePath(index, baseLayerName);
				if (File.Exists(path))
				{
					return (Bitmap)Bitmap.FromFile(path);
				}
				else
					return null;
			}
			catch (Exception ex)
			{
				Trace.TraceError("GetTileFromDisc: " + ex.Message);
				return null;
			}

		}
		private void SaveTileToDisc(Bitmap image, TileIndex index, string baseLayerName)
		{
			try
			{
				string path = GetTilePath(index, baseLayerName);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
			}
			catch (Exception ex)
			{
				Trace.TraceError("SaveTileToDisc: " + ex.Message);
			}
		}
		private string GetTilePath(TileIndex index, string baseLayerName)
		{
			return Path.Combine(_rootDir, baseLayerName, index.Z.ToString(), index.X.ToString(), index.Y.ToString() + ".png");
		}

		#region IDisposable Membres

		public void Dispose()
		{
			try
			{
				_imageCacheTimer.Stop();
				_imageCacheTimer.Dispose();
				if (_imageCache != null)
				{
					foreach (string baseLayer in _imageCache.Keys)
					{
						foreach (var kvp in _imageCache[baseLayer])
						{
							kvp.Value.Dispose();
						}
					}
					_imageCache.Clear();
				}
			}
			catch (Exception exDispose)
			{
				Trace.TraceWarning("TileDownloader.Dispose: " + exDispose.Message);
			}

		}

		#endregion
	}
}
