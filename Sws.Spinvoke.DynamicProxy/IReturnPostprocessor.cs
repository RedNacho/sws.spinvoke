using System;

namespace Sws.Spinvoke.DynamicProxy
{
	public interface IReturnPostprocessor
	{
		bool CanProcess(object output, Type requiredReturnType);
		object Process(object output, Type requiredReturnType);
	}
}

