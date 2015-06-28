using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

using Moq;

using Sws.Spinvoke.Interception.ReturnPostprocessing;
using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class PointerToStringReturnPostprocessorTests
	{
		[Test ()]
		public void CanProcessReturnsTrueForIntPtrAndRequiredStringType ()
		{
			var subject = new PointerToStringReturnPostprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var canProcess = subject.CanProcess (new IntPtr (0), typeof(string));

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForNonIntPtrOutput ()
		{
			var subject = new PointerToStringReturnPostprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var canProcess = subject.CanProcess (new object(), typeof(string));

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForRequiredTypeNotAssignableFromString ()
		{
			var subject = new PointerToStringReturnPostprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var canProcess = subject.CanProcess (new IntPtr (0), typeof(int));

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void PreallocatedPointerIsMarshaledBackToString()
		{
			const string TestString = "I am a string.";

			var subject = new PointerToStringReturnPostprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var ptr = Marshal.StringToHGlobalAuto (TestString);

			var result = (string) subject.Process (ptr, typeof(string));

			Marshal.FreeHGlobal (ptr);

			Assert.AreEqual (TestString, result);
		}
	}
}

