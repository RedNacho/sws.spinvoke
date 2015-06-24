using System;

using DynamicProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

namespace Sws.Spinvoke.Interception.DynamicProxy
{
	public class ProxyGenerator : IProxyGenerator
	{
		private readonly DynamicProxyGenerator _sourceProxyGenerator;

		public ProxyGenerator(DynamicProxyGenerator sourceProxyGenerator)
		{
			if (sourceProxyGenerator == null) {
				throw new ArgumentNullException ("sourceProxyGenerator");
			}

			_sourceProxyGenerator = sourceProxyGenerator;
		}

		public bool AllowsTarget
		{
			get { return true; }
		}

		public T CreateProxy<T> (IInterceptor interceptor) where T : class
		{
			return _sourceProxyGenerator.CreateInterfaceProxyWithoutTarget<T> (new SpinvokeInterceptor (interceptor));
		}

		public object CreateProxy (Type interfaceToProxy, IInterceptor interceptor)
		{
			return _sourceProxyGenerator.CreateInterfaceProxyWithoutTarget (interfaceToProxy, new SpinvokeInterceptor (interceptor));
		}

		public T CreateProxyWithTarget<T>(IInterceptor interceptor, T target) where T : class
		{
			return _sourceProxyGenerator.CreateInterfaceProxyWithTarget<T> (target, new SpinvokeInterceptor(interceptor));
		}

		public object CreateProxyWithTarget(Type interfaceToProxy, IInterceptor interceptor, object target)
		{
			return _sourceProxyGenerator.CreateInterfaceProxyWithTarget (interfaceToProxy, target, new SpinvokeInterceptor(interceptor));
		}
	}
}

