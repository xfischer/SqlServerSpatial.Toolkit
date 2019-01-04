using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Diagnostics
{
	public class NotifyPropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
		{
			if (PropertyChanged != null)
			{
				var lambda = (LambdaExpression)property;
				MemberExpression memberExpression;
				if (lambda.Body is UnaryExpression)
				{
					var unaryExpression = (UnaryExpression)lambda.Body;
					memberExpression = (MemberExpression)unaryExpression.Operand;
				}
				else
				{
					memberExpression = (MemberExpression)lambda.Body;
				}
				PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
			}
		}
	}
}
