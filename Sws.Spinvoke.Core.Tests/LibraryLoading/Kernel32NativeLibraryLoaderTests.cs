using System;
using NUnit.Framework;
using Moq;
using Sws.Spinvoke.Core.LibraryLoading;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class Kernel32NativeLibraryLoaderTests
	{
		[Test ()]
		public void LoadLibraryCallsLoadLibrary()
		{
			const string TestLibraryName = "mytestlib";

			var safeLibraryHandle = new TestSafeLibraryHandle ();

			var kernel32Mock = new Mock<IKernel32Managed<TestSafeLibraryHandle>> ();

			kernel32Mock.Setup (kernel32 => kernel32.LoadLibrary (TestLibraryName))
				.Returns (safeLibraryHandle);

			var nativeLibraryLoader = new Kernel32NativeLibraryLoader<TestSafeLibraryHandle> (kernel32Mock.Object);

			var actualSafeLibraryHandle = nativeLibraryLoader.LoadLibrary (TestLibraryName);

			kernel32Mock.Verify (kernel32 => kernel32.LoadLibrary (TestLibraryName), Times.Once);
			kernel32Mock.Verify (kernel32 => kernel32.LoadLibrary (It.IsAny<string>()), Times.Once);
			kernel32Mock.Verify (kernel32 => kernel32.GetProcAddress (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Never);

			Assert.AreEqual (safeLibraryHandle, actualSafeLibraryHandle);
		}

		[Test ()]
		public void UnloadLibraryCallsCloseOnSafeLibraryHandleFromLoadLibrary()
		{
			bool handleWasClosedBeforeUnload;
			bool handleWasClosedAfterUnload;

			const string TestLibraryName = "mytestlib";

			var safeLibraryHandle = new TestSafeLibraryHandle ();

			var kernel32Mock = new Mock<IKernel32Managed<TestSafeLibraryHandle>> ();

			kernel32Mock.Setup (kernel32 => kernel32.LoadLibrary (TestLibraryName))
				.Returns (safeLibraryHandle);

			var nativeLibraryLoader = new Kernel32NativeLibraryLoader<TestSafeLibraryHandle> (kernel32Mock.Object);

			var actualSafeLibraryHandle = nativeLibraryLoader.LoadLibrary (TestLibraryName);

			handleWasClosedBeforeUnload = actualSafeLibraryHandle.IsClosed;

			nativeLibraryLoader.UnloadLibrary (actualSafeLibraryHandle);

			handleWasClosedAfterUnload = actualSafeLibraryHandle.IsClosed;

			Assert.IsFalse (handleWasClosedBeforeUnload);
			Assert.IsTrue (handleWasClosedAfterUnload);

			kernel32Mock.Verify (kernel32 => kernel32.LoadLibrary (It.IsAny<string>()), Times.Once);
			kernel32Mock.Verify (kernel32 => kernel32.GetProcAddress (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Never);
		}

		[Test ()]
		public void GetFunctionPointerCallsGetProcAddress()
		{
			const string TestFunctionName = "testfunction";

			var expectedFunctionPtr = new IntPtr (12345);

			var testHandle = new TestSafeLibraryHandle ();

			var kernel32Mock = new Mock<IKernel32Managed<TestSafeLibraryHandle>> ();

			kernel32Mock.Setup (kernel32 => kernel32.GetProcAddress (testHandle, TestFunctionName))
				.Returns (expectedFunctionPtr);

			var kernel32NativeLibraryLoader = new Kernel32NativeLibraryLoader<TestSafeLibraryHandle> (kernel32Mock.Object);

			var actualFunctionPtr = kernel32NativeLibraryLoader.GetFunctionPointer (testHandle, TestFunctionName);

			Assert.AreEqual (expectedFunctionPtr, actualFunctionPtr);

			kernel32Mock.Verify (kernel32 => kernel32.GetProcAddress (testHandle, TestFunctionName), Times.Once);
			kernel32Mock.Verify (kernel32 => kernel32.GetProcAddress (It.IsAny<SafeHandle>(), It.IsAny<string>()), Times.Once);
			kernel32Mock.Verify (kernel32 => kernel32.LoadLibrary (It.IsAny<string>()), Times.Never);
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