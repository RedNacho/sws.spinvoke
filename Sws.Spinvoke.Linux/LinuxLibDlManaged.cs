using System;

using Sws.Spinvoke.Core.LibraryLoading;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Linux
{
	public class LinuxLibDlManaged : ILibDlManaged<LinuxLibDlManaged.LinuxSafeLibraryHandle>
	{
		public LinuxSafeLibraryHandle DlOpen (string filename, int flags)
		{
			return dlopen (filename, flags);
		}

		public int DlClose (IntPtr handle)
		{
			return dlclose (handle);
		}

		public IntPtr DlError ()
		{
			return dlerror ();
		}

		public IntPtr DlSym (SafeHandle handle, string symbol)
		{
			return dlsym (handle, symbol);
		}

		public string StringFromDlError (IntPtr dlErrorPtr)
		{
			return Marshal.PtrToStringAuto (dlErrorPtr);
		}

		[DllImport("libdl.so")]
		private static extern LinuxSafeLibraryHandle dlopen(string fileName, int flags);

		[DllImport("libdl.so")]
		private static extern int dlclose(IntPtr handle);

		[DllImport("libdl.so")]
		private static extern IntPtr dlerror();

		[DllImport("libdl.so")]
		private static extern IntPtr dlsym(SafeHandle handle, string symbol);

		public sealed class LinuxSafeLibraryHandle : SafeLibraryHandle
		{
			protected override bool ReleaseHandle ()
			{
				var result = dlclose (handle);

				return (result == 0);
			}
		}
	}
}

