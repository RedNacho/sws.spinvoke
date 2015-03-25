using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.Exceptions;

// This is really easy to implement for Windows.
// I just haven't done it because I can't test it right now.
namespace Sws.Spinvoke.Linux
{
	public class LinuxNativeLibraryLoader : INativeLibraryLoader
	{
		private const int RTLD_NOW = 2;

		public SafeLibraryHandle LoadLibrary (string fileName)
		{
			return dlopen (fileName, RTLD_NOW);
		}

		public void UnloadLibrary (SafeLibraryHandle libHandle)
		{
			libHandle.Close ();
		}

		public IntPtr GetFunctionPointer (SafeLibraryHandle libHandle, string functionName)
		{
			dlerror ();

			var res = dlsym (libHandle, functionName);

			var errPtr = dlerror ();

			if (errPtr != IntPtr.Zero) {
				throw new NativeLibraryLoadException (string.Format ("dlsym errored trying to load function {0}.", functionName));
			}

			return res;
		}

		[DllImport("libdl.so")]
		private static extern LinuxSafeLibraryHandle dlopen(string fileName, int flags);

		[DllImport("libdl.so")]
		private static extern int dlclose(IntPtr handle);

		[DllImport("libdl.so")]
		private static extern IntPtr dlerror();

		[DllImport("libdl.so")]
		private static extern IntPtr dlsym(SafeHandle handle, string symbol);

		private sealed class LinuxSafeLibraryHandle : SafeLibraryHandle
		{
			protected override bool ReleaseHandle ()
			{
				var result = dlclose (handle);

				return (result == 0);
			}
		}
	}
}

