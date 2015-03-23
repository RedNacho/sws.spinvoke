using System;

using Ninject.Activation;

using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject.Providers
{
	public class NativeProxyProvider<T> : Provider<T>
		where T : class
	{
		private readonly Func<NativeProxyProviderConfiguration<T>> _configurationSource;

		public NativeProxyProvider(Func<NativeProxyProviderConfiguration<T>> configurationSource)
		{
			if (configurationSource == null)
				throw new ArgumentNullException("configurationSource");

			_configurationSource = configurationSource;
		}

		protected override T CreateInstance (IContext context)
		{
			var configuration = _configurationSource ();

			var nativeDelegateInterceptorContext = new NativeDelegateInterceptorContext (configuration.LibraryName, configuration.CallingConvention, configuration.NativeDelegateResolver);

			var nativeDelegateInterceptor = configuration.NativeDelegateInterceptorFactory.CreateInterceptor(nativeDelegateInterceptorContext);

			var nonNativeFallback = configuration.NonNativeFallbackSource (new NonNativeFallbackContext(nativeDelegateInterceptorContext, context));

			if (nonNativeFallback == null) {
				if (configuration.ServiceType == typeof(T)) {
					return configuration.ProxyGenerator.CreateProxy<T> (nativeDelegateInterceptor);
				} else {
					return configuration.ProxyGenerator.CreateProxy (configuration.ServiceType, nativeDelegateInterceptor) as T;
				}
			} else {
				if (configuration.ServiceType == typeof(T)) {
					return configuration.ProxyGenerator.CreateProxyWithTarget<T> (nativeDelegateInterceptor, nonNativeFallback);
				} else {
					return configuration.ProxyGenerator.CreateProxyWithTarget (configuration.ServiceType, nativeDelegateInterceptor, nonNativeFallback) as T;
				}
			}
		}
	}
}

