using System;
using NUnit.Framework;
using Moq;
using Sws.Spinvoke.Core.LibraryLoading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core.Exceptions;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class LibDlNativeLibraryLoaderTests
	{
		private const int RTLD_NOW = 2;

		[Test ()]
		public void LoadLibraryCallsDlLoadWithRtldNow()
		{
			const string TestLibraryName = "mytestlib";

			var safeLibraryHandle = new TestSafeLibraryHandle ();

			var libDlMock = new Mock<ILibDlManaged<TestSafeLibraryHandle>> ();

			libDlMock.Setup (libDl => libDl.DlOpen (TestLibraryName, RTLD_NOW))
				.Returns (safeLibraryHandle);

			var nativeLibraryLoader = new LibDlNativeLibraryLoader<TestSafeLibraryHandle> (libDlMock.Object);

			var actualSafeLibraryHandle = nativeLibraryLoader.LoadLibrary (TestLibraryName);

			libDlMock.Verify (libDl => libDl.DlOpen (TestLibraryName, RTLD_NOW), Times.Once);
			libDlMock.Verify (libDl => libDl.DlOpen (It.IsAny<string>(), It.IsAny<int>()), Times.Once);
			libDlMock.Verify (libDl => libDl.DlError (), Times.Never);
			libDlMock.Verify (libDl => libDl.DlSym (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Never);
			libDlMock.Verify (libDl => libDl.StringFromDlError (It.IsAny<IntPtr> ()), Times.Never);

			Assert.AreEqual (safeLibraryHandle, actualSafeLibraryHandle);
		}

		[Test ()]
		public void UnloadLibraryCallsCloseOnSafeLibraryHandleFromLoadLibrary()
		{
			bool handleWasClosedBeforeUnload;
			bool handleWasClosedAfterUnload;

			const string TestLibraryName = "mytestlib";

			var safeLibraryHandle = new TestSafeLibraryHandle ();

			var libDlMock = new Mock<ILibDlManaged<TestSafeLibraryHandle>> ();

			libDlMock.Setup (libDl => libDl.DlOpen (TestLibraryName, RTLD_NOW))
				.Returns (safeLibraryHandle);

			var nativeLibraryLoader = new LibDlNativeLibraryLoader<TestSafeLibraryHandle> (libDlMock.Object);

			var actualSafeLibraryHandle = nativeLibraryLoader.LoadLibrary (TestLibraryName);

			handleWasClosedBeforeUnload = actualSafeLibraryHandle.IsClosed;

			nativeLibraryLoader.UnloadLibrary (actualSafeLibraryHandle);

			handleWasClosedAfterUnload = actualSafeLibraryHandle.IsClosed;

			Assert.IsFalse (handleWasClosedBeforeUnload);
			Assert.IsTrue (handleWasClosedAfterUnload);

			libDlMock.Verify (libDl => libDl.DlOpen (It.IsAny<string>(), It.IsAny<int>()), Times.Once);
			libDlMock.Verify (libDl => libDl.DlError (), Times.Never);
			libDlMock.Verify (libDl => libDl.DlSym (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Never);
			libDlMock.Verify (libDl => libDl.StringFromDlError (It.IsAny<IntPtr> ()), Times.Never);
		}

		[Test ()]
		public void GetFunctionPointerClearsDlErrorThenCallsDlSymThenChecksDlError()
		{
			const string TestFunctionName = "testfunction";

			var expectedFunctionPtr = new IntPtr (12345);

			var calls = new List<string> ();

			var testHandle = new TestSafeLibraryHandle ();

			var libDlMock = new Mock<ILibDlManaged<TestSafeLibraryHandle>> ();

			libDlMock.Setup (libDl => libDl.DlError ())
				.Callback (() => calls.Add ("dlerror"))
				.Returns (IntPtr.Zero);

			libDlMock.Setup (libDl => libDl.DlSym (testHandle, TestFunctionName))
				.Callback (() => calls.Add ("dlsym"))
				.Returns (expectedFunctionPtr);

			var libDlNativeLibraryLoader = new LibDlNativeLibraryLoader<TestSafeLibraryHandle> (libDlMock.Object);

			var actualFunctionPtr = libDlNativeLibraryLoader.GetFunctionPointer (testHandle, TestFunctionName);

			Assert.AreEqual (expectedFunctionPtr, actualFunctionPtr);

			Assert.AreEqual (3, calls.Count);

			Assert.AreEqual ("dlerror", calls [0]);
			Assert.AreEqual ("dlsym", calls [1]);
			Assert.AreEqual ("dlerror", calls [2]);

			libDlMock.Verify (libDl => libDl.DlError (), Times.Exactly(2));
			libDlMock.Verify (libDl => libDl.DlSym (testHandle, TestFunctionName), Times.Once);
			libDlMock.Verify (libDl => libDl.DlSym (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Once);
			libDlMock.Verify (libDl => libDl.DlOpen (It.IsAny<string>(), It.IsAny<int>()), Times.Never);
			libDlMock.Verify (libDl => libDl.StringFromDlError (It.IsAny<IntPtr> ()), Times.Never);
		}

		[Test ()]
		public void GetFunctionPointerClearsDlErrorThenCallsDlSymThenChecksDlErrorAndThrowsExceptionIfError()
		{
			Exception caughtException = null;

			const string TestFunctionName = "testfunction";

			const string ExpectedErrorString = "testerrorstring";

			var expectedFunctionPtr = new IntPtr (12345);

			var expectedErrorPtr = new IntPtr (67890);

			var calls = new List<string> ();

			var testHandle = new TestSafeLibraryHandle ();

			var libDlMock = new Mock<ILibDlManaged<TestSafeLibraryHandle>> ();

			var errorPtrs = new [] { IntPtr.Zero, expectedErrorPtr };

			var errorCallCounter = 0;

			libDlMock.Setup (libDl => libDl.DlError ())
				.Callback (() => calls.Add ("dlerror"))
				.Returns (() => errorPtrs[errorCallCounter++]);

			libDlMock.Setup (libDl => libDl.DlSym (testHandle, TestFunctionName))
				.Callback (() => calls.Add ("dlsym"))
				.Returns (expectedFunctionPtr);

			libDlMock.Setup (libDl => libDl.StringFromDlError (expectedErrorPtr))
				.Callback (() => calls.Add ("stringfromdlerror"))
				.Returns (ExpectedErrorString);

			var libDlNativeLibraryLoader = new LibDlNativeLibraryLoader<TestSafeLibraryHandle> (libDlMock.Object);

			try {
			    libDlNativeLibraryLoader.GetFunctionPointer (testHandle, TestFunctionName);
			}
			catch (Exception ex) {
				caughtException = ex;	
			}

			Assert.IsNotNull (caughtException);

			Assert.IsTrue (caughtException is NativeLibraryLoadException);

			Assert.AreEqual (4, calls.Count);

			Assert.AreEqual ("dlerror", calls [0]);
			Assert.AreEqual ("dlsym", calls [1]);
			Assert.AreEqual ("dlerror", calls [2]);
			Assert.AreEqual ("stringfromdlerror", calls [3]);

			libDlMock.Verify (libDl => libDl.DlError (), Times.Exactly(2));
			libDlMock.Verify (libDl => libDl.DlSym (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Once);
			libDlMock.Verify (libDl => libDl.StringFromDlError (expectedErrorPtr), Times.Once);
			libDlMock.Verify (libDl => libDl.StringFromDlError (It.IsAny<IntPtr>()), Times.Once);
			libDlMock.Verify (libDl => libDl.DlOpen (It.IsAny<string>(), It.IsAny<int>()), Times.Never);
		}

		public sealed class TestSafeLibraryHandle : SafeLibraryHandle
		{
			protected override bool ReleaseHandle ()
			{
				return true;
			}
		}
	}
}

