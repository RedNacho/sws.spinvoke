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
			var subject = new DelegateToPointerArgumentPreprocessor ();

			AddOneDelegate addOne = i => i + 1;

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

			AddOneDelegate addOne = i => i + 1;

			var ptr = (IntPtr) subject.Process (addOne);

			var delegateFromPtr = Marshal.GetDelegateForFunctionPointer (ptr, typeof(AddOneDelegate));

			var result = delegateFromPtr.DynamicInvoke (new object[] { 7 });

			Assert.AreEqual (8, result);
		}

		[Test ()]
		public void DelegateIsConvertedIfNecessaryContextSupplied()
		{
			var subject = new DelegateToPointerArgumentPreprocessor ();

			var register = new List<Delegate> ();

			var delegateTypeToDelegateSignatureConverterMock = new Mock<IDelegateTypeToDelegateSignatureConverter> ();

			var delegateSignature = new DelegateSignature (new [] { typeof(int) }, typeof(int), CallingConvention.Cdecl);

			delegateTypeToDelegateSignatureConverterMock.Setup (
				dttdsc => dttdsc.CreateDelegateSignature (typeof(Func<int, int>), CallingConvention.Cdecl))
					.Returns (delegateSignature);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (delegateSignature))
				.Returns (typeof(AddOneDelegate));

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

			// Decorate the context with the stuff we do care about.
			argumentPreprocessorContext = argumentPreprocessorContext.DecorateWith (
				DelegateToPointerArgumentPreprocessor.CreateContextDecoration(
					delegateTypeToDelegateSignatureConverterMock.Object,
					delegateTypeProviderMock.Object,
					CallingConvention.Cdecl,
					register.Add
				)	
			);

			subject.SetContext (argumentPreprocessorContext);

			Func<int, int> addOne = i => i + 1;

			var ptr = (IntPtr) subject.Process (addOne);

			var delegateFromPtr = Marshal.GetDelegateForFunctionPointer (ptr, typeof(AddOneDelegate));

			var result = delegateFromPtr.DynamicInvoke (new object[] { 7 });

			Assert.AreEqual (8, result);

			Assert.IsInstanceOf<AddOneDelegate> (delegateFromPtr);

			Assert.AreEqual (1, register.Count);

			Assert.AreEqual (delegateFromPtr, register[0]);
		}

		public delegate int AddOneDelegate(int i);
	}
}

