using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject.Providers
{
	public class NativeProxyProviderConfiguration<T>
	{
		public Type ServiceType { get; set; }

		public string LibraryName { get; set; }

		public Func<NonNativeFallbackContext, T> NonNativeFallbackSource { get; set; }

		public CallingConvention CallingConvention { get; set; }

		public INativeDelegateResolver NativeDelegateResolver { get; set; }

		public INativeDelegateInterceptorFactory NativeDelegateInterceptorFactory { get; set; }

		public IProxyGenerator ProxyGenerator { get; set; }
	}
}

