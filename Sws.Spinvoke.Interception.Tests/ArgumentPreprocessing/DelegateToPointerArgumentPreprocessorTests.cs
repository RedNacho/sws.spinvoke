using NUnit.Framework;

using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Moq;
using Sws.Spinvoke.Core;
using System.Collections.Generic;
using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class DelegateToPointerArgumentPreprocessorTests
	{
		[Test ()]
		public void CanProcessReturnsTrueForDelegate()
		{
			var subject = new DelegateToPointerArgumentPreprocessor (Mock.Of<IContextualArgumentPreprocessor> ());

			AddOneDelegate addOne = i => i + 1;

			var canProcess = subject.CanProcess (addOne);

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForNonDelegate()
		{
			var subject = new DelegateToPointerArgumentPreprocessor (Mock.Of<IContextualArgumentPreprocessor> ());

			var canProcess = subject.CanProcess (new object ());

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void DelegateCanBeMarshaledBackFromPointer()
		{
			var subject = new DelegateToPointerArgumentPreprocessor (Mock.Of<IContextualArgumentPreprocessor> ());

			AddOneDelegate addOne = i => i + 1;

			var ptr = (IntPtr) subject.Process (addOne);

			var delegateFromPtr = Marshal.GetDelegateForFunctionPointer (ptr, typeof(AddOneDelegate));

			var result = delegateFromPtr.DynamicInvoke (new object[] { 7 });

			Assert.AreEqual (8, result);
		}

		[Test ()]
		public void DelegateIsConvertedIfAllowedByTheDelegateToUnmanagedFunctionArgumentPreprocessor()
		{
			var delegateToUnmanagedFunctionArgumentPreprocessorMock = new Mock<IContextualArgumentPreprocessor> ();

			var subject = new DelegateToPointerArgumentPreprocessor (delegateToUnmanagedFunctionArgumentPreprocessorMock.Object);

			Func<int, int> addOne = i => i + 1;

			AddOneDelegate unmanagedAddOne = i => i + 1;

			delegateToUnmanagedFunctionArgumentPreprocessorMock.Setup (a => a.CanProcess(addOne)).Returns (true);
			delegateToUnmanagedFunctionArgumentPreprocessorMock.Setup (a => a.Process(addOne)).Returns (unmanagedAddOne);

			// Completely dummy context, we don't care about any of these values.
			var argumentPreprocessorContext = new ArgumentPreprocessorContext (Mock.Of<IInvocation> (), new NativeDelegateMapping(
				true,
				"dummy",
				"dummy",
				CallingConvention.Cdecl,
				new IArgumentPreprocessor[0],
				typeof(AddOneDelegate),
				new Type[0],
				typeof(int),
				Mock.Of<IReturnPostprocessor>()
			), 0);

			subject.SetContext (argumentPreprocessorContext);

			delegateToUnmanagedFunctionArgumentPreprocessorMock.Verify (d => d.SetContext (argumentPreprocessorContext));

			var ptr = (IntPtr) subject.Process (addOne);

			var delegateFromPtr = Marshal.GetDelegateForFunctionPointer (ptr, typeof(AddOneDelegate));

			var result = delegateFromPtr.DynamicInvoke (new object[] { 7 });

			Assert.AreEqual (8, result);

			Assert.IsInstanceOf<AddOneDelegate> (delegateFromPtr);

			Assert.AreEqual (delegateFromPtr, unmanagedAddOne);
		}

		public delegate int AddOneDelegate(int i);
	}
}

