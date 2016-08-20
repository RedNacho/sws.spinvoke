using System;
using Sws.Spinvoke.Core.Facade;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Bootstrapper
{
	public static class Bootstrapper
	{
		private static readonly Func<INativeLibraryLoader> NativeLibraryLoaderFactory =
			(new NativeLibraryLoaderFactory (OSDetector.DetectOS)).Create;

		private static readonly Func<INativeLibraryLoader, INativeLibraryLoader> NativeLibraryLoaderVerifier =
			(new NativeLibraryLoaderVerifier (OSDetector.DetectOS)).VerifyNativeLibraryLoader;
		
		public static SpinvokeCoreFacade.Builder CreateCoreFacadeBuilderForOS() {
			return new SpinvokeCoreFacade.Builder (
				NativeLibraryLoaderVerifier (NativeLibraryLoaderFactory ()));
		}

		public static SpinvokeCoreFacade CreateCoreFacadeForOS() {
			return CreateCoreFacadeBuilderForOS ().Build ();
		}

	}
}

