using System;

using Ninject;
using Ninject.Infrastructure;

using Sws.Spinvoke.Core;

// PoC: Unit test, refactor.
namespace Sws.Spinvoke.Ninject.Extensions
{
	public static class SpinvokeNinjectExtensionsConfiguration
	{
		public static void Configure (INativeLibraryLoader nativeLibraryLoader)
		{
			INativeDelegateResolver nativeDelegateResolver;

			using (var kernel = new StandardKernel ()) {
				kernel.Bind<INativeLibraryLoader> ().ToConstant (nativeLibraryLoader);
				kernel.Load (new SpinvokeModule (StandardScopeCallbacks.Transient, "BindingToSyntaxExtensions"));
				nativeDelegateResolver = kernel.Get<INativeDelegateResolver> ();
			}

			BindingToSyntaxExtensions.Configure (nativeDelegateResolver);
		}
	}
}

