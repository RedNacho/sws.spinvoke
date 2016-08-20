using NUnit.Framework;
using System;
using Moq;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Bootstrapper.Tests
{
	[TestFixture ()]
	public class NativeLibraryLoaderVerifierTests
	{
		private INativeLibraryLoader SetupNativeLibraryLoader<TDelegate>(string libraryName,
			string functionName,
			TDelegate functionToExecute
		) {
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader> ();

			var safeLibraryHandleMock = new Mock<SafeLibraryHandle> ();

			var ptr = Marshal.GetFunctionPointerForDelegate (functionToExecute);

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary (libraryName))
				.Returns (safeLibraryHandleMock.Object);

			nativeLibraryLoaderMock.Setup (nll => nll.GetFunctionPointer (safeLibraryHandleMock.Object, functionName))
				.Returns (ptr);

			return nativeLibraryLoaderMock.Object;
		}

		private delegate int GetPidDelegate();
	
		[Test ()]
		public void AcceptsIntegerResultOfGetPidWhenOSIsX11 ()
		{
			int getPidCallCounts = 0;

			Func<int> getPid = () => {
				getPidCallCounts++;
				return 1;
			};

			var nativeLibraryLoader = SetupNativeLibraryLoader ("libc.so", "getpid", getPid);

			var result = (new NativeLibraryLoaderVerifier (() => OS.X11))
				.VerifyNativeLibraryLoader (nativeLibraryLoader);

			Assert.AreEqual(1, getPidCallCounts);
			Assert.AreEqual (nativeLibraryLoader, result);
		}

		[Test ()]
		public void FailsOnExceptionFromGetPidWhenOSIsX11 ()
		{
			GetPidDelegate getPid = () => {
				throw new Exception ("GetPid isn't working");
			};

			var nativeLibraryLoader = SetupNativeLibraryLoader ("libc.so", "getpid", getPid);

			Assert.Throws<InvalidOperationException>(() =>
				(new NativeLibraryLoaderVerifier (() => OS.X11))
					.VerifyNativeLibraryLoader (nativeLibraryLoader));
		}
	}
}