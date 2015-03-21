using System;
using System.Runtime.InteropServices;

using Ninject.Activation;

using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Ninject.Extensions
{
	public interface INonNativeFallbackContext
	{
		IContext NinjectContext { get; }
		string LibraryName { get; }
		CallingConvention CallingConvention { get; }
		INativeDelegateResolver NativeDelegateResolver { get; }
	}
}

