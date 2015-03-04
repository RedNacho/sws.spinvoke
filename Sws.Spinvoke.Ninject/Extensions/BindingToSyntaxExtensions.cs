using System;

using Castle.DynamicProxy;

using Ninject;
using Ninject.Activation;
using Ninject.Infrastructure;
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

		public static void Configure(INativeLibraryLoader nativeLibraryLoader)
		{
			using (var kernel = new StandardKernel ()) {
				kernel.Bind<INativeLibraryLoader> ().ToConstant (nativeLibraryLoader);
				kernel.Load (new SpinvokeModule (StandardScopeCallbacks.Transient, "BindingToSyntaxExtensions"));
				NativeDelegateResolver = kernel.Get<INativeDelegateResolver> ();
			}
		}

		private static void VerifyConfigured()
		{
			if (NativeDelegateResolver == null) {
				throw new InvalidOperationException ("You must call BindingToSyntaxExtensions.Configure first");
			}
		}

		public static IBindingWhenInNamedWithOrOnSyntax<T> ToNative<T>(this IBindingToSyntax<T> bindingToSyntax, string libraryName)
			where T : class
		{
			VerifyConfigured ();

			bindingToSyntax.BindingConfiguration.ProviderCallback = context => new NativeProxyProvider<T> (libraryName);

			return new BindingConfigurationBuilder<T> (bindingToSyntax.BindingConfiguration, typeof(T).Format(), bindingToSyntax.Kernel);
		}

		private class NativeProxyProvider<T> : Provider<T>
			where T : class
		{
			private readonly string _libraryName;

			public NativeProxyProvider(string libraryName)
			{
				if (libraryName == null)
					throw new ArgumentNullException("libraryName");

				_libraryName = libraryName;
			}

			protected override T CreateInstance (IContext context)
			{
				return ProxyGenerator.CreateInterfaceProxyWithoutTarget<T> (new NativeDelegateInterceptor (_libraryName, NativeDelegateResolver));
			}
		}
	}
}

