using System;
using Sws.Spinvoke.Core.Exceptions;

namespace Sws.Spinvoke.Core.LibraryLoading
{
	public class LibDlNativeLibraryLoader : INativeLibraryLoader
	{
		private const int RTLD_NOW = 2;

		private readonly ILibDlManaged _libDlManaged;

		public LibDlNativeLibraryLoader(ILibDlManaged libDlManaged)
		{
			this._libDlManaged = libDlManaged;
		}

		public SafeLibraryHandle LoadLibrary (string fileName)
		{
			return _libDlManaged.DlOpen (fileName, RTLD_NOW);
		}

		public void UnloadLibrary (SafeLibraryHandle libHandle)
		{
			libHandle.Close ();
		}

		public IntPtr GetFunctionPointer (SafeLibraryHandle libHandle, string functionName)
		{
			_libDlManaged.DlError ();

			var res = _libDlManaged.DlSym (libHandle, functionName);

			var errPtr = _libDlManaged.DlError ();

			if (errPtr != IntPtr.Zero) {
				throw new NativeLibraryLoadException (string.Format ("dlsym errored trying to load function {0}.  Error was: {1}", functionName, _libDlManaged.StringFromDlError(errPtr)));
			}

			return res;
		}
	}
}
