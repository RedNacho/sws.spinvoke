using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Windows
{
	// THIS HAS NOT YET BEEN TESTED ON WINDOWS
	public class WindowsNativeLibraryLoader : INativeLibraryLoader
	{
		public SafeLibraryHandle LoadLibrary (string fileName)
		{
			return NativeMethods.LoadLibrary (fileName);
		}

		public void UnloadLibrary (SafeLibraryHandle libHandle)
		{
			libHandle.Close ();
		}

		public IntPtr GetFunctionPointer (SafeLibraryHandle libHandle, string functionName)
		{
			return NativeMethods.GetProcAddress (libHandle, functionName);
		}

		private static class NativeMethods
		{
			[DllImport("kernel32.dll")]
			public static extern WindowsSafeLibraryHandle LoadLibrary(string dllToLoad);

			[DllImport("kernel32.dll")]
			public static extern IntPtr GetProcAddress(SafeHandle hModule, string procedureName);

			[DllImport("kernel32.dll")]
			public static extern bool FreeLibrary(IntPtr hModule);
		}

		private sealed class WindowsSafeLibraryHandle : SafeLibraryHandle
		{
			protected override bool ReleaseHandle ()
			{
				return NativeMethods.FreeLibrary (handle);
			}
		}
	}
}

