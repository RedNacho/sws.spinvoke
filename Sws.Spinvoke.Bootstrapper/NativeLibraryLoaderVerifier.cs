using System;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Bootstrapper
{
	public class NativeLibraryLoaderVerifier
	{
		private delegate int GetPidDelegate ();

		private delegate IntPtr GetCurrentProcessDelegate ();

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
			case OS.Windows:
				TestNativeLibraryLoader<GetCurrentProcessDelegate> (os,
					nativeLibraryLoader,
					"Kernel32.dll",
					"GetCurrentProcess",
					new object[0],
					output => output != null && output is IntPtr);
				break;
			case OS.Mac:
				TestNativeLibraryLoader<GetPidDelegate> (os,
					nativeLibraryLoader,
					"libc.dylib",
					"getpid",
					new object[0],
					output => output != null && output is int);
				break;
			default:
				throw new InvalidOperationException(string.Format("Cannot verify INativeLibraryLoader instance; OS {0} is not recognised. If this isn't your OS, the bootstrapper has made a mistake.", os));
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

			string failureMessage = string.Format ("INativeLibraryLoader test call (library {1}, function {2}) did not work correctly. Is your OS {0}? If not, the bootstrapper has made a mistake.", os, libraryName, functionName);

			if (innerException != null) {
				throw new InvalidOperationException (failureMessage, innerException);
			} else {
				throw new InvalidOperationException (failureMessage);
			}
		}
	}
}

