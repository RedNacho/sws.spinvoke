using System;

using Ninject.Activation;

using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject.Providers
{
	public class NativeProxyProvider<T> : Provider<T>
		where T : class
	{
		private readonly NativeProxyProviderConfiguration<T> _configuration;

		public NativeProxyProvider(NativeProxyProviderConfiguration<T> configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			_configuration = configuration;
		}

		protected override T CreateInstance (IContext context)
		{
			var configuration = _configuration;

			var nativeDelegateInterceptorContext = new NativeDelegateInterceptorContext (
				                                       configuration.LibraryName,
				                                       configuration.CallingConvention,
				                                       configuration.NativeDelegateResolver,
				                                       configuration.ArgumentPreprocessorContextCustomiser,
				                                       configuration.ReturnPostprocessorContextCustomiser
			                                       );

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

