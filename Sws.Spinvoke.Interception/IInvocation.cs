using System;
using System.Reflection;

namespace Sws.Spinvoke.Interception
{
	public interface IInvocation
	{
		object[] Arguments { get; }
		MethodInfo Method { get; }
		object ReturnValue { get; set; }
	}
}

