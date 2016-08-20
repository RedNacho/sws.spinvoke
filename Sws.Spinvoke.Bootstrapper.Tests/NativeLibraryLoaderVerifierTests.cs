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
		private INativeLibraryLoader SetupNativeLibraryLoader(string libraryName,
			string functionName,
			Delegate functionToExecute
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
	
		private delegate IntPtr GetCurrentProcessDelegate();

		[Test ()]
		public void AcceptsIntegerResultOfGetPidWhenOSIsX11 ()
		{
			int getPidCallCounts = 0;

			GetPidDelegate getPid = () => {
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
		public void AcceptsIntPtrResultOfGetCurrentProcessWhenOSIsWindows ()
		{
			int getCurrentProcessCallCounts = 0;

			GetCurrentProcessDelegate getCurrentProcess = () => {
				getCurrentProcessCallCounts++;
				return new IntPtr(1);
			};

			var nativeLibraryLoader = SetupNativeLibraryLoader ("Kernel32.dll", "GetCurrentProcess", getCurrentProcess);

			var result = (new NativeLibraryLoaderVerifier (() => OS.Windows))
				.VerifyNativeLibraryLoader (nativeLibraryLoader);

			Assert.AreEqual(1, getCurrentProcessCallCounts);
			Assert.AreEqual (nativeLibraryLoader, result);
		}

		[Test ()]
		public void AcceptsIntegerResultOfGetPidWhenOSIsMac ()
		{
			int getPidCallCounts = 0;

			GetPidDelegate getPid = () => {
				getPidCallCounts++;
				return 1;
			};

			var nativeLibraryLoader = SetupNativeLibraryLoader ("libc.dylib", "getpid", getPid);

			var result = (new NativeLibraryLoaderVerifier (() => OS.Mac))
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

		[Test ()]
		public void FailsOnExceptionFromGetCurrentProcessWhenOSIsWindows ()
		{
			GetCurrentProcessDelegate getCurrentProcess = () => {
				throw new Exception ("GetCurrentProcess isn't working");
			};

			var nativeLibraryLoader = SetupNativeLibraryLoader ("Kernel32.dll", "GetCurrentProcess", getCurrentProcess);

			Assert.Throws<InvalidOperationException>(() =>
				(new NativeLibraryLoaderVerifier (() => OS.Windows))
					.VerifyNativeLibraryLoader (nativeLibraryLoader));
		}

		[Test ()]
		public void FailsOnExceptionFromGetPidWhenOSIsMac ()
		{
			GetPidDelegate getPid = () => {
				throw new Exception ("GetPid isn't working");
			};

			var nativeLibraryLoader = SetupNativeLibraryLoader ("libc.dylib", "getpid", getPid);

			Assert.Throws<InvalidOperationException>(() =>
				(new NativeLibraryLoaderVerifier (() => OS.Mac))
					.VerifyNativeLibraryLoader (nativeLibraryLoader));
		}

		[Test ()]
		public void AlwaysFailsWhenOSIsUnrecognised ()
		{
			Assert.Throws<InvalidOperationException>(() =>
				(new NativeLibraryLoaderVerifier (() => OS.Other))
					.VerifyNativeLibraryLoader (Mock.Of<INativeLibraryLoader>()));
		}
	}
}