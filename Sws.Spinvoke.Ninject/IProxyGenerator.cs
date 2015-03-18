using System;

using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject
{
	public interface IProxyGenerator
	{
		T CreateProxy<T>(IInterceptor interceptor);
		object CreateProxy(Type interfaceToProxy, IInterceptor interceptor);
	}
}

