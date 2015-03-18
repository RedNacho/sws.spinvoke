using System;

namespace Sws.Spinvoke.Interception
{
	public interface IProxyGenerator
	{ 
		T CreateProxy<T> (IInterceptor interceptor) where T : class;
		object CreateProxy(Type interfaceToProxy, IInterceptor interceptor);
	}
}

