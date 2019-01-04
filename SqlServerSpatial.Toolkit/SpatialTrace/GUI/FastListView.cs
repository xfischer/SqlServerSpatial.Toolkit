using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetTopologySuite.Diagnostics
{
	/// <summary>
	/// Faster listView for SelectedItems manipulation
	/// Ideal in case you're adding / removing al lot of selected elements
	/// <see cref="http://stackoverflow.com/a/21941834/1818237"/>
	/// </summary>
	public class FastListView : ListView
	{
		public void SelectItems(IEnumerable items)
		{
			SetSelectedItems(items);
		}
	}
}
