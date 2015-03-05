using System;
using System.Runtime.InteropServices;

using Castle.DynamicProxy;

using Ninject.Activation;
using Ninject.Infrastructure.Introspection;
using Ninject.Planning.Bindings;
using Ninject.Syntax;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.DynamicProxy;

// PoC - unit test, refactor.
namespace Sws.Spinvoke.Ninject.Extensions
{
	public static class BindingToSyntaxExtensions
	{
		private static INativeDelegateResolver NativeDelegateResolver;

		private static ProxyGenerator ProxyGenerator = new ProxyGenerator();

		internal static void Configure(INativeDelegateResolver nativeDelegateResolver)
		{
			NativeDelegateResolver = nativeDelegateResolver;
		}

		private static void VerifyConfigured()
		{
			if (NativeDelegateResolver == null) {
				throw new InvalidOperationException ("You must call SpinvokeNinjectExtensionsConfiguration.Configure first");
			}
		}

		public static IBindingWhenInNamedWithOrOnSyntax<T> ToNative<T>(this IBindingToSyntax<T> bindingToSyntax, string libraryName, CallingConvention? callingConvention = null)
			where T : class
		{
			Type serviceType;

			var bindingBuilder = bindingToSyntax as BindingBuilder<T>;

			if (bindingBuilder == null) {
				serviceType = typeof(T);
			} else {
				serviceType = bindingBuilder.Binding.Service;
			}

			return ToNative (bindingToSyntax, serviceType, libraryName, callingConvention);
		}

		public static IBindingWhenInNamedWithOrOnSyntax<T> ToNative<T>(this IBindingToSyntax<T> bindingToSyntax, Type serviceType, string libraryName, CallingConvention? callingConvention = null)
			where T : class
		{
			VerifyConfigured ();

			bindingToSyntax.BindingConfiguration.ProviderCallback = context => new NativeProxyProvider<T> (serviceType, libraryName, callingConvention.GetValueOrDefault(CallingConvention.Winapi));

			return new BindingConfigurationBuilder<T> (bindingToSyntax.BindingConfiguration, serviceType.Format(), bindingToSyntax.Kernel);
		}

		private class NativeProxyProvider<T> : Provider<T>
			where T : class
		{
			private readonly Type _serviceType;

			private readonly string _libraryName;

			private readonly CallingConvention _callingConvention;

			public NativeProxyProvider(Type serviceType, string libraryName, CallingConvention callingConvention)
			{
				if (serviceType == null)
					throw new ArgumentNullException("serviceType");

				if (libraryName == null)
					throw new ArgumentNullException("libraryName");

				_serviceType = serviceType;
				_libraryName = libraryName;
				_callingConvention = callingConvention;
			}

			protected override T CreateInstance (IContext context)
			{
				var nativeDelegateInterceptor = new NativeDelegateInterceptor (_libraryName, _callingConvention, NativeDelegateResolver);

				if (_serviceType == typeof(T)) {
					return ProxyGenerator.CreateInterfaceProxyWithoutTarget<T> (nativeDelegateInterceptor);
				} else {
					return ProxyGenerator.CreateInterfaceProxyWithoutTarget (_serviceType, nativeDelegateInterceptor) as T;
				}
			}
		}
	}
}

