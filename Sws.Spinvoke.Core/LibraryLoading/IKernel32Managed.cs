using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.LibraryLoading
{
	public interface IKernel32Managed<TSafeLibraryHandle>
		where TSafeLibraryHandle : SafeLibraryHandle
	{
		TSafeLibraryHandle LoadLibrary (string fileName);
		IntPtr GetProcAddress (SafeHandle hModule, string procedureName);
	}
}

