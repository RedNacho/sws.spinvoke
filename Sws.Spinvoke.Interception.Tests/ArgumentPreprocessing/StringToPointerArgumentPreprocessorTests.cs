using NUnit.Framework;
using System;

using Moq;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.MemoryManagement;

using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class StringToPointerArgumentPreprocessorTests
	{
		[Test ()]
		public void CanProcessReturnsTrueForString ()
		{
			var subject = new StringToPointerArgumentPreprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var canProcess = subject.CanProcess ("Test");

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForNonString()
		{
			var subject = new StringToPointerArgumentPreprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var canProcess = subject.CanProcess (new object ());

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void StringCanBeMarshaledBackFromPointer()
		{
			const string TestString = "Hello!";

			var subject = new StringToPointerArgumentPreprocessor (PointerManagementMode.DoNotDestroy, Mock.Of<PointerMemoryManager> ());

			var ptr = (IntPtr) subject.Process (TestString);

			var storedString = Marshal.PtrToStringAuto (ptr);

			Marshal.FreeHGlobal (ptr);

			Assert.AreEqual (TestString, storedString);
		}
	}
}

