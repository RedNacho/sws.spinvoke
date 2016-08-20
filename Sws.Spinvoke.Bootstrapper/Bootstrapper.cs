using System;
using Sws.Spinvoke.Core.Facade;
using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception.Facade;
using Sws.Spinvoke.Interception;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Linux;

namespace Sws.Spinvoke.Bootstrapper
{
	public static class Bootstrapper
	{
		private delegate int GetPidDelegate ();

		private static readonly Func<INativeLibraryLoader> NativeLibraryLoaderFactory =
			(new NativeLibraryLoaderFactory(OSDetector.DetectOS)).Create;

		private static readonly Func<INativeLibraryLoader, INativeLibraryLoader> NativeLibraryLoaderVerifier =
			(new NativeLibraryLoaderVerifier (OSDetector.DetectOS)).VerifyNativeLibraryLoader;
		
		public static SpinvokeCoreFacade.Builder CreateCoreFacadeBuilderForOS() {
			return new SpinvokeCoreFacade.Builder (NativeLibraryLoaderVerifier (
				NativeLibraryLoaderFactory ()));
		}

		public static SpinvokeCoreFacade CreateCoreFacadeForOS() {
			return CreateCoreFacadeBuilderForOS ().Build ();
		}

	}
}

