using System;

namespace Sws.Spinvoke.Core.LibraryLoading
{
	public class Kernel32NativeLibraryLoader<TSafeLibraryHandle> : INativeLibraryLoader
		where TSafeLibraryHandle : SafeLibraryHandle
	{
		private readonly IKernel32Managed<TSafeLibraryHandle> _kernel32Managed;

		public Kernel32NativeLibraryLoader (IKernel32Managed<TSafeLibraryHandle> kernel32Managed)
		{
			_kernel32Managed = kernel32Managed;
		}

		public SafeLibraryHandle LoadLibrary (string fileName)
		{
			return _kernel32Managed.LoadLibrary (fileName);
		}

		public void UnloadLibrary (SafeLibraryHandle libHandle)
		{
			libHandle.Close ();
		}

		public IntPtr GetFunctionPointer (SafeLibraryHandle libHandle, string functionName)
		{
			return _kernel32Managed.GetProcAddress (libHandle, functionName);
		}

	}
}

