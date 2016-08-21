using System;

namespace Sws.Spinvoke.Core
{
	public interface INativeLibraryLoader
	{
		SafeLibraryHandle LoadLibrary(string fileName);
		void UnloadLibrary(SafeLibraryHandle libHandle);
		IntPtr GetFunctionPointer(SafeLibraryHandle libHandle, string functionName);
	}
}

