using System;
using Sws.Spinvoke.Core.LibraryLoading;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Windows
{
	public class WindowsKernel32Managed : IKernel32Managed
	{
		public SafeLibraryHandle LoadLibrary (string fileName)
		{
			return NativeMethods.LoadLibrary (fileName);
		}

		public IntPtr GetProcAddress (SafeHandle hModule, string procedureName)
		{
			return NativeMethods.GetProcAddress (hModule, procedureName);
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

