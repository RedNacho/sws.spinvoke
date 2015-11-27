using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.LibraryLoading
{
	public interface ILibDlManaged<TSafeLibraryHandle>
		where TSafeLibraryHandle : SafeLibraryHandle
	{
		TSafeLibraryHandle DlOpen(string filename, int flags);
		IntPtr DlError();
		IntPtr DlSym(SafeHandle handle, string symbol);
		string StringFromDlError(IntPtr dlErrorPtr);
	}
}