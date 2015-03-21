using System;
using System.Runtime.InteropServices;

using Ninject.Activation;
using Ninject.Infrastructure.Introspection;
using Ninject.Planning.Bindings;
using Ninject.Syntax;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;

// PoC - unit test, refactor.
namespace Sws.Spinvoke.Ninject.Extensions
{
	public static class BindingToSyntaxExtensions
	{
		private static INativeDelegateResolver NativeDelegateResolver;

		private static IProxyGenerator ProxyGenerator;

		internal static void Configure(INativeDelegateResolver nativeDelegateResolver, IProxyGenerator proxyGenerator)
		{
			NativeDelegateResolver = nativeDelegateResolver;
			ProxyGenerator = proxyGenerator;
		}

		private static void VerifyConfigured()
		{
			if (NativeDelegateResolver == null || ProxyGenerator == null) {
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

			CallingConvention? callingConvention = null;

			Func<INonNativeFallbackContext, T> nonNativeFallbackSource = context => null;

			bindingToSyntax.BindingConfiguration.ProviderCallback = context => new NativeProxyProvider<T> (serviceType, libraryName, () => callingConvention.GetValueOrDefault(CallingConvention.Winapi), nonNativeFallbackSource);

			return new SpinvokeBindingConfigurationBuilder<T> (
				bindingToSyntax.BindingConfiguration,
				serviceType.Format(),
				bindingToSyntax.Kernel,
				cc => callingConvention = cc,
				nnfs => {
					if (!ProxyGenerator.AllowsTarget) {
						throw new InvalidOperationException("In order to allow a non native fallback to be supplied, the proxy generator must support a target.");
					}

					nonNativeFallbackSource = nnfs;
				});
		}

		private class NativeProxyProvider<T> : Provider<T>
			where T : class
		{
			private readonly Type _serviceType;

			private readonly string _libraryName;

			private readonly Func<CallingConvention> _callingConventionSource;

			private readonly Func<INonNativeFallbackContext, T> _nonNativeFallbackSource;

			public NativeProxyProvider(Type serviceType, string libraryName, Func<CallingConvention> callingConventionSource, Func<INonNativeFallbackContext, T> nonNativeFallbackSource)
			{
				if (serviceType == null)
					throw new ArgumentNullException("serviceType");

				if (libraryName == null)
					throw new ArgumentNullException("libraryName");

				if (callingConventionSource == null)
					throw new ArgumentNullException("callingConventionSource");

				if (nonNativeFallbackSource == null)
					throw new ArgumentNullException("nonNativeFallbackSource");

				_serviceType = serviceType;
				_libraryName = libraryName;
				_callingConventionSource = callingConventionSource;
				_nonNativeFallbackSource = nonNativeFallbackSource;
			}

			protected override T CreateInstance (IContext context)
			{
				var callingConvention = _callingConventionSource ();

				var nativeDelegateInterceptor = new NativeDelegateInterceptor (_libraryName, callingConvention, NativeDelegateResolver);

				var nonNativeFallback = _nonNativeFallbackSource (new NonNativeFallbackContext () {
					NinjectContext = context,
					LibraryName = _libraryName,
					CallingConvention = callingConvention,
					NativeDelegateResolver = NativeDelegateResolver
				});

				if (nonNativeFallback == null) {
					if (_serviceType == typeof(T)) {
						return ProxyGenerator.CreateProxy<T> (nativeDelegateInterceptor);
					} else {
						return ProxyGenerator.CreateProxy (_serviceType, nativeDelegateInterceptor) as T;
					}
				} else {
					if (_serviceType == typeof(T)) {
						return ProxyGenerator.CreateProxyWithTarget<T> (nativeDelegateInterceptor, nonNativeFallback);
					} else {
						return ProxyGenerator.CreateProxyWithTarget (_serviceType, nativeDelegateInterceptor, nonNativeFallback) as T;
					}
				}
			}

			private class NonNativeFallbackContext : INonNativeFallbackContext
			{
				public IContext NinjectContext {
					get;
					set;
				}

				public string LibraryName {
					get;
					set;
				}

				public CallingConvention CallingConvention {
					get;
					set;
				}

				public INativeDelegateResolver NativeDelegateResolver {
					get;
					set;
				}
			}
		}
	}
}

