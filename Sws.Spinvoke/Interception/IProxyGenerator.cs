using System;

namespace Sws.Spinvoke.Interception
{
	public interface IProxyGenerator
	{ 
		bool AllowsTarget { get; }
		T CreateProxy<T> (IInterceptor interceptor) where T : class;
		object CreateProxy(Type interfaceToProxy, IInterceptor interceptor);
		T CreateProxyWithTarget<T> (IInterceptor interceptor, T target) where T : class;
		object CreateProxyWithTarget(Type interfaceToProxy, IInterceptor interceptor, object target);
	}
}

