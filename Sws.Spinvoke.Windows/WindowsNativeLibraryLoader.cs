using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.LibraryLoading;

namespace Sws.Spinvoke.Windows
{
	// BRIEFLY TESTED ON THE NEAREST COPY OF WINDOWS.
	public class WindowsNativeLibraryLoader : Kernel32NativeLibraryLoader<WindowsKernel32Managed.WindowsSafeLibraryHandle>
	{
		public WindowsNativeLibraryLoader() : base(new WindowsKernel32Managed())
		{
		}
	}
}

