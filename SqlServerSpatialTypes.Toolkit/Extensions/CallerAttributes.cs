using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NET45
#else
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public class CallerMemberNameAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public class CallerFilePathAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public class CallerLineNumberAttribute : Attribute
	{
	}
}
#endif
