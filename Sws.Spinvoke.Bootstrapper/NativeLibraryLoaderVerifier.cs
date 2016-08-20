using System;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Bootstrapper
{
	public class NativeLibraryLoaderVerifier
	{
		private delegate int GetPidDelegate ();

		private readonly Func<OS> _osDetector;

		public NativeLibraryLoaderVerifier(Func<OS> osDetector) {
			this._osDetector = osDetector;
		}

		public INativeLibraryLoader VerifyNativeLibraryLoader(INativeLibraryLoader nativeLibraryLoader) {
			return VerifyNativeLibraryLoader (_osDetector (), nativeLibraryLoader);
		}

		private static INativeLibraryLoader VerifyNativeLibraryLoader(OS os, INativeLibraryLoader nativeLibraryLoader) {
			switch (os) {
			case OS.X11:
				TestNativeLibraryLoader<GetPidDelegate> (os,
					nativeLibraryLoader,
					"libc.so",
					"getpid",
					new object[0],
					output => output != null && output is int);
				break;
			}
			return nativeLibraryLoader;
		}

		private static void TestNativeLibraryLoader<TTestFunctionType>(
			OS os,
			INativeLibraryLoader nativeLibraryLoader,
			string libraryName,
			string functionName,
			object[] args,
			Predicate<object> responseVerifier)
		{
			Exception innerException = null;

			bool everythingOk = false;

			SafeLibraryHandle library = null;

			try {
				library = nativeLibraryLoader.LoadLibrary (libraryName);

				var functionPtr = nativeLibraryLoader.GetFunctionPointer (library, functionName);

				var function = Marshal.GetDelegateForFunctionPointer (functionPtr, typeof(TTestFunctionType));

				var output = function.DynamicInvoke (args);

				everythingOk = responseVerifier(output);
			} catch (Exception ex) {
				innerException = ex;
			} finally {
				if (library != null) {
					nativeLibraryLoader.UnloadLibrary (library);
				}
			}

			if (everythingOk) {
				return;
			}

			string failureMessage = string.Format ("INativeLibraryLoader test call did not work correctly. Is your OS {0}? If not, the bootstrapper has made a mistake.", os);

			if (innerException != null) {
				throw new InvalidOperationException (failureMessage, innerException);
			} else {
				throw new InvalidOperationException (failureMessage);
			}
		}
	}
}

