using System;
using System.Runtime.InteropServices;

using Ninject.Infrastructure.Introspection;
using Ninject.Planning.Bindings;
using 	Ninject.Syntax;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.Ninject.Providers;

// PoC - unit test, refactor.
namespace Sws.Spinvoke.Ninject.Extensions
{
	public static class BindingToSyntaxExtensions
	{
		private static INativeDelegateResolver DefaultNativeDelegateResolver;

		private static IProxyGenerator DefaultProxyGenerator;

		private static INativeDelegateInterceptorFactory DefaultNativeDelegateInterceptorFactory;

		internal static void Configure(INativeDelegateResolver nativeDelegateResolver, IProxyGenerator proxyGenerator, INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory)
		{
			DefaultNativeDelegateResolver = nativeDelegateResolver;
			DefaultProxyGenerator = proxyGenerator;
			DefaultNativeDelegateInterceptorFactory = nativeDelegateInterceptorFactory;
		}

		private static void VerifyConfigured()
		{
			if (DefaultNativeDelegateResolver == null || DefaultProxyGenerator == null || DefaultNativeDelegateInterceptorFactory == null) {
				throw new InvalidOperationException ("You must call SpinvokeNinjectExtensionsConfiguration.Configure first");
			}
		}

		public static ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> ToNative<T>(this IBindingToSyntax<T> bindingToSyntax, string libraryName)
			where T : class
		{
			Type serviceType;

			var bindingBuilder = bindingToSyntax as BindingBuilder<T>;

			if (bindingBuilder == null) {
				serviceType = typeof(T);
			} else {
				serviceType = bindingBuilder.Binding.Service;
			}

			return ToNative (bindingToSyntax, serviceType, libraryName);
		}

		public static ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> ToNative<T>(this IBindingToSyntax<T> bindingToSyntax, Type serviceType, string libraryName)
			where T : class
		{
			VerifyConfigured ();

			var nativeProxyProviderConfiguration = new NativeProxyProviderConfiguration<T> () {
					CallingConvention = CallingConvention.Winapi,
					LibraryName = libraryName,
					NativeDelegateInterceptorFactory = DefaultNativeDelegateInterceptorFactory,
					NativeDelegateResolver = DefaultNativeDelegateResolver,
					NonNativeFallbackSource = context => null,
					ProxyGenerator = DefaultProxyGenerator,
					ServiceType = serviceType };

			bindingToSyntax.BindingConfiguration.ProviderCallback = context => new NativeProxyProvider<T> (nativeProxyProviderConfiguration);

			return new SpinvokeBindingConfigurationBuilder<T> (
				bindingToSyntax.BindingConfiguration,
				serviceType.Format(),
				bindingToSyntax.Kernel,
				cc => nativeProxyProviderConfiguration.CallingConvention = cc,
				nnfs => {
					if (!DefaultProxyGenerator.AllowsTarget) {
						throw new InvalidOperationException("In order to allow a non native fallback to be supplied, the proxy generator must support a target.");
					}

					nativeProxyProviderConfiguration.NonNativeFallbackSource = nnfs;
				},
				ndr => nativeProxyProviderConfiguration.NativeDelegateResolver = ndr,
				ndif => nativeProxyProviderConfiguration.NativeDelegateInterceptorFactory = ndif,
				pg => nativeProxyProviderConfiguration.ProxyGenerator = pg
			);
		}
	}
}

