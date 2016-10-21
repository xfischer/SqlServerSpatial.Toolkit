using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace SqlServerSpatial.Toolkit.Viewers
{
	public interface ITraceViewModel
	{
		bool GroupsEnabled { get; set; }

		ObservableCollection<TraceLineDesign> Traces { get; set; }

		ICollectionView TracesView { get; set; }
		string Filter { get; set; }
	}
}