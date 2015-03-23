using NUnit.Framework;

using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class DelegateToPointerArgumentPreprocessorTests
	{
		[Test ()]
		public void CanProcessReturnsTrueForDelegate()
		{
			var subject = new DelegateToPointerArgumentPreprocessor ();

			Func<int, int> addOne = i => i + 1;

			var canProcess = subject.CanProcess (addOne);

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForNonDelegate()
		{
			var subject = new DelegateToPointerArgumentPreprocessor ();

			var canProcess = subject.CanProcess (new object ());

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void DelegateCanBeMarshaledBackFromPointer()
		{
			var subject = new DelegateToPointerArgumentPreprocessor ();

			Func<int, int> addOne = i => i + 1;

			var ptr = (IntPtr) subject.Process (addOne);

			var delegateFromPtr = Marshal.GetDelegateForFunctionPointer (ptr, typeof(AddOneDelegate));

			var result = delegateFromPtr.DynamicInvoke (new object[] { 7 });

			Assert.AreEqual (8, result);
		}

		public delegate int AddOneDelegate(int i);
	}
}

