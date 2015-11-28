using Sws.Spinvoke.Core.LibraryLoading;

namespace Sws.Spinvoke.Windows
{
	// BRIEFLY TESTED ON THE NEAREST COPY OF WINDOWS.
	public class WindowsNativeLibraryLoader : Kernel32NativeLibraryLoader
	{
		public WindowsNativeLibraryLoader() : base(new WindowsKernel32Managed())
		{
		}
	}
}

