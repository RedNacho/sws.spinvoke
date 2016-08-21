using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.LibraryLoading
{
	public interface IKernel32Managed
	{
		SafeLibraryHandle LoadLibrary (string fileName);
		IntPtr GetProcAddress (SafeHandle hModule, string procedureName);
	}
}

