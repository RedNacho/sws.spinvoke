using System;
using Sws.Spinvoke.Core.Facade;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Bootstrapper
{
	public static class Bootstrapper
	{
		private static Func<INativeLibraryLoader> _nativeLibraryLoaderFactory =
			(new NativeLibraryLoaderFactory (OSDetector.DetectOS)).Create;

		private static Func<INativeLibraryLoader, INativeLibraryLoader> _nativeLibraryLoaderVerifier =
			(new NativeLibraryLoaderVerifier (OSDetector.DetectOS)).VerifyNativeLibraryLoader;

		private static bool _verificationEnabled = true;

		/// <summary>
		/// Disables verification of the native library loader by calling a native function.
		/// </summary>
		public static void DisableVerification() {
			_verificationEnabled = false;
		}

		/// <summary>
		/// Enables verification of the native library loader by calling a native function.
		/// </summary>
		public static void EnableVerification() {
			_verificationEnabled = true;
		}

		public static bool VerificationEnabled {
			get { return _verificationEnabled; }
		}

		/// <summary>
		/// Swaps out the func which verifies that the native library loader works correctly.
		/// </summary>
		/// <param name="verifier">Verifier.</param>
		public static void ChangeVerification(Func<INativeLibraryLoader, INativeLibraryLoader> verifier) {
			_nativeLibraryLoaderVerifier = verifier;
		}

		/// <summary>
		/// Swaps out the native library loader factory, which creates the loader based on the OS.
		/// </summary>
		/// <param name="factory">Factory.</param>
		public static void ChangeNativeLibraryLoaderFactory(Func<INativeLibraryLoader> factory) {
			_nativeLibraryLoaderFactory = factory;
		}

		/// <summary>
		/// Retrieves the current native library loader factory (useful if you want to decorate it or something).
		/// </summary>
		/// <returns>The native library loader factory.</returns>
		public static Func<INativeLibraryLoader> NativeLibraryLoaderFactory {
			get { return _nativeLibraryLoaderFactory; }
		}

		/// <summary>
		/// Retrieves the current native library loader verifier (useful if you want to decorate it or something).
		/// </summary>
		/// <returns>The native library loader verifier.</returns>
		public static Func<INativeLibraryLoader, INativeLibraryLoader> NativeLibraryLoaderVerifier {
			get { return _nativeLibraryLoaderVerifier; }
		}

		/// <summary>
		/// Creates a Spinvoke Core facade builder for the current OS, which can be configured further as necessary.
		/// </summary>
		/// <returns>The core facade builder for OS.</returns>
		public static SpinvokeCoreFacade.Builder CreateCoreFacadeBuilderForOS() {
			return new SpinvokeCoreFacade.Builder (
				CreateNativeLibraryLoaderForOS ());
		}

		/// <summary>
		/// Creates a Spinvoke Core facade for the current OS.
		/// </summary>
		/// <returns>The core facade for OS.</returns>
		public static SpinvokeCoreFacade CreateCoreFacadeForOS() {
			return CreateCoreFacadeBuilderForOS ().Build ();
		}

		/// <summary>
		/// Creates a native library loader for the current OS. This can then be used
		/// with e.g. the Ninject extensions.
		/// </summary>
		/// <returns>The native library loader for OS.</returns>
		public static INativeLibraryLoader CreateNativeLibraryLoaderForOS() {
			return VerifyNativeLibraryLoader (_nativeLibraryLoaderFactory ());
		}

		private static INativeLibraryLoader VerifyNativeLibraryLoader(INativeLibraryLoader nativeLibraryLoader) {
			if (!_verificationEnabled) {
				return nativeLibraryLoader;
			}

			return _nativeLibraryLoaderVerifier (nativeLibraryLoader);
		}
	}
}