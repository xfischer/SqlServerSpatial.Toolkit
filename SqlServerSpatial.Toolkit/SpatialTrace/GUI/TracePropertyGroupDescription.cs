using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetTopologySuite.Diagnostics
{
	public class TracePropertyGroupDescription : PropertyGroupDescription
	{
		public TracePropertyGroupDescription(string propertyName)
			: base(propertyName)
		{
		}

		public override object GroupNameFromItem(object item, int level, CultureInfo culture)
		{
			string name = ((TraceLineDesign)item).Indent;// base.GroupNameFromItem(item, level, culture);
			return name;
		}
	}
}
