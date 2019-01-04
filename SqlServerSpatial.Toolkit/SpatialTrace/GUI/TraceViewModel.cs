using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetTopologySuite.Diagnostics.Viewers
{
	public class TraceViewModel : NotifyPropertyChangedBase, ITraceViewModel
	{

		private bool _groupsEnabled = true;

		public bool GroupsEnabled
		{
			get { return _groupsEnabled; }
			set {
				if (value != _groupsEnabled)
				{
					_groupsEnabled = value;
					SetGrouping();
					NotifyOfPropertyChange(() => TracesView);
				}
			}
		}
		
		private ObservableCollection<TraceLineDesign> _traces;
		public ObservableCollection<TraceLineDesign> Traces
		{
			get
			{
				return _traces;
			}
			set
			{
				_traces = value;

				SetGrouping();
				
				NotifyOfPropertyChange(() => TracesView);
			}
		}

		private void SetGrouping()
		{
			_tracesView = CollectionViewSource.GetDefaultView(_traces);
			if (_groupsEnabled)
			{
				if (_traces.Any(t => !string.IsNullOrWhiteSpace(t.Indent)))
				{
					PropertyGroupDescription groupDescription = new PropertyGroupDescription("Indent");
					_tracesView.GroupDescriptions.Add(groupDescription);
				}
			}
			else
			{
				_tracesView.GroupDescriptions.Clear();
			}
		}

		private ICollectionView _tracesView;
		public ICollectionView TracesView
		{
			get
			{
				return _tracesView;
			}
			set
			{
				_tracesView = value;
				NotifyOfPropertyChange(() => TracesView);				
			}
		}


		private string _filter = null;
		public string Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;
				if (string.IsNullOrWhiteSpace(_filter))
					_tracesView.Filter = null;
				else
				_tracesView.Filter = FilterTrace;
			}
		}

		private bool FilterTrace(object obj)
		{
			TraceLineDesign v_trace = obj as TraceLineDesign;
			if (v_trace == null)
				return true;

			return v_trace.Indent.ToLower().Contains(_filter);



		}
	}
}
