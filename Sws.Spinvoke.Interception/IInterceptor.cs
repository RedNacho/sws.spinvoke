using System;

namespace Sws.Spinvoke.Interception
{
	public interface IInterceptor
	{
		void Intercept (IInvocation invocation);
	}
}

