using Sws.Spinvoke.Core.LibraryLoading;

namespace Sws.Spinvoke.Linux
{
	public class LinuxNativeLibraryLoader : LibDlNativeLibraryLoader
	{
		public LinuxNativeLibraryLoader() : base(new LinuxLibDlManaged())
		{
		}
	}
}

