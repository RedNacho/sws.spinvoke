using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Sws.Spinvoke.DynamicProxy.Tests
{
	[TestFixture ()]
	public class PointerToStructReturnPostprocessorTests
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct TestStruct
		{
			public int x;

			public int y;

			public int z;
		}

		[Test ()]
		public void CanProcessReturnsTrueForIntPtrAndRequiredValueType()
		{
			var subject = new PointerToStructReturnPostprocessor ();

			var output = new IntPtr (0);

			var canProcess = subject.CanProcess (output, typeof(TestStruct));

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForNonIntPtr()
		{
			var subject = new PointerToStructReturnPostprocessor ();

			const int Output = 75;

			var canProcess = subject.CanProcess (Output, typeof(TestStruct));

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForRequiredReferenceType()
		{
			var subject = new PointerToStructReturnPostprocessor ();

			var output = new IntPtr (0);

			var canProcess = subject.CanProcess (output, typeof(object));

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void PreallocatedPointerIsMarshaledBackToStruct()
		{
			var subject = new PointerToStructReturnPostprocessor ();

			var ptr = Marshal.AllocHGlobal (Marshal.SizeOf<TestStruct>());

			var testData = new TestStruct () { x = 7, y = 8, z = 9 };

			Marshal.StructureToPtr (testData, ptr, true);

			var result = subject.Process (ptr, typeof(TestStruct));

			Assert.AreEqual (testData, result);
		}
	}
}

