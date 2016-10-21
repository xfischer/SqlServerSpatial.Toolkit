using SqlServerSpatial.Toolkit.BaseLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerSpatial.Toolkit.Viewers
{
	public class MapViewModel : NotifyPropertyChangedBase, IMapViewModel
	{
		private List<IBaseLayer> _registeredBaseLayers;
		public List<IBaseLayer> BaseLayers
		{
			get
			{
				return _registeredBaseLayers;
			}
			private set
			{
				_registeredBaseLayers = value;
				NotifyOfPropertyChange(() => BaseLayers);
			}
		}

		private IBaseLayer _baseLayer;
		private readonly IBaseLayerViewer _baseLayerViewer;
		public IBaseLayer BaseLayer
		{
			get { return _baseLayer; }
			set
			{
				if (value is EmptyBaseLayer)
				{
					_baseLayer = null;
				}
				else
				{
					_baseLayer = value;
				}

				NotifyOfPropertyChange(() => BaseLayer);

				SetBaseLayer(_baseLayer);
			}
		}

		private void SetBaseLayer(IBaseLayer baseLayer)
		{
			if (_baseLayerViewer != null)
			{
				_baseLayerViewer.Enabled = baseLayer != null;
				_baseLayerViewer.SetBaseLayer(baseLayer);
			}
		}

		public MapViewModel(IBaseLayerViewer baseLayerViewer)
		{
			Initialize();
			_baseLayerViewer = baseLayerViewer;
		}

		private void Initialize()
		{
			_registeredBaseLayers = new List<IBaseLayer>();
			IBaseLayer v_emptyBaseLayer = new EmptyBaseLayer();
			_registeredBaseLayers.Add(v_emptyBaseLayer);
			_registeredBaseLayers.Add(new ZXYBaseLayer("http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png", "OSM (Mapnik)", true, true));
			BaseLayers = _registeredBaseLayers;
			_baseLayer = v_emptyBaseLayer;

		}
	}
}