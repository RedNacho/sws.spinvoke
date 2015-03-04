using System;

namespace Sws.Spinvoke.Core
{
	public interface INativeLibraryLoader
	{
		IntPtr LoadLibrary(string fileName);
		void UnloadLibrary(IntPtr libHandle);
		IntPtr GetFunctionPointer(IntPtr libHandle, string functionName);
	}
}

