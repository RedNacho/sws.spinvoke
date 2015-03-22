using System;

using Ninject;
using Ninject.Infrastructure;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;

// PoC: Unit test, refactor.
namespace Sws.Spinvoke.Ninject.Extensions
{
	public static class SpinvokeNinjectExtensionsConfiguration
	{
		public static void Configure (INativeLibraryLoader nativeLibraryLoader, IProxyGenerator proxyGenerator, INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory = null)
		{
			INativeDelegateResolver nativeDelegateResolver;

			using (var kernel = new StandardKernel ()) {
				kernel.Bind<INativeLibraryLoader> ().ToConstant (nativeLibraryLoader);
				kernel.Load (new SpinvokeModule (StandardScopeCallbacks.Transient, "BindingToSyntaxExtensions"));
				nativeDelegateResolver = kernel.Get<INativeDelegateResolver> ();
			}

			Configure (nativeDelegateResolver, proxyGenerator, nativeDelegateInterceptorFactory);
		}

		public static void Configure (INativeDelegateResolver nativeDelegateResolver, IProxyGenerator proxyGenerator, INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory = null)
		{
			if (nativeDelegateInterceptorFactory == null) {
				using (var kernel = new StandardKernel ()) {
					kernel.Load (new SpinvokeInterceptionModule (StandardScopeCallbacks.Transient));
					nativeDelegateInterceptorFactory = kernel.Get<INativeDelegateInterceptorFactory> ();
				}
			}

			BindingToSyntaxExtensions.Configure (nativeDelegateResolver, proxyGenerator, nativeDelegateInterceptorFactory);
		}
	}
}

