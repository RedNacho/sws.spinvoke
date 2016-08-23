using NUnit.Framework;
using System;
using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Moq;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Interception.ReturnPostprocessing;
using Sws.Spinvoke.Core;
using System.Collections.Generic;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class DelegateToInteropCompatibleDelegateArgumentPreprocessorTests
	{
		private ArgumentPreprocessorContext CreateDummyContext() {
			return CreateDummyContext (CallingConvention.Cdecl);
		}

		private ArgumentPreprocessorContext CreateDummyContext(CallingConvention callingConvention) {
			// Mostly dummy context, we don't care about any of these values except the calling convention.
			return new ArgumentPreprocessorContext (Mock.Of<IInvocation> (), new NativeDelegateMapping(
					true,
					"dummy",
					"dummy",
					callingConvention,
					new IArgumentPreprocessor[0],
					typeof(AddOneDelegate),
					new Type[0],
					typeof(int),
					Mock.Of<IReturnPostprocessor>()
				), 0);
		}

		[Test ()]
		public void CanProcessReturnsTrueForDelegateAndCustomisedContext()
		{
			var subject = new DelegateToInteropCompatibleDelegateArgumentPreprocessor (null);

			Func<int, int> addOne = i => i + 1;

			subject.SetContext (CreateDummyContext().Customise (
				DelegateToInteropCompatibleDelegateArgumentPreprocessor.CreateContextCustomisation (
					Mock.Of<IDelegateTypeToDelegateSignatureConverter> (),
					Mock.Of<IDelegateTypeProvider> ()
				)
			));

			var canProcess = subject.CanProcess (addOne);

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForNonDelegate()
		{
			var subject = new DelegateToInteropCompatibleDelegateArgumentPreprocessor (null);

			subject.SetContext (CreateDummyContext().Customise (
				DelegateToInteropCompatibleDelegateArgumentPreprocessor.CreateContextCustomisation (
					Mock.Of<IDelegateTypeToDelegateSignatureConverter> (),
					Mock.Of<IDelegateTypeProvider> ()
				)
			));

			var canProcess = subject.CanProcess (new object());

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void CanProcessReturnsFalseForDelegateWithoutCustomisedContext()
		{
			var subject = new DelegateToInteropCompatibleDelegateArgumentPreprocessor (null);

			Func<int, int> addOne = i => i + 1;

			subject.SetContext (CreateDummyContext());

			var canProcess = subject.CanProcess (addOne);

			Assert.IsFalse (canProcess);
		}

		[Test ()]
		public void DelegateIsConvertedToInteropCompatibleDelegate()
		{
			DelegateIsConvertedToInteropCompatibleDelegate (CallingConvention.Cdecl, null);
		}

		[Test ()]
		public void DelegateIsConvertedToInteropCompatibleDelegateWithOverriddenCallingConvention() {
			DelegateIsConvertedToInteropCompatibleDelegate (CallingConvention.Cdecl, CallingConvention.Winapi);
		}

		[Test ()]
		public void CallingConventionOfNativeCallIsAssumedIfNoCallingConventionSpecified() {
			DelegateIsConvertedToInteropCompatibleDelegate (null, null);
		}

		private void DelegateIsConvertedToInteropCompatibleDelegate(CallingConvention? contextCallingConvention, CallingConvention? argumentCallingConvention)
		{
			var nativeCallCallingConvention = CallingConvention.FastCall;

			var expectedCallingConvention = argumentCallingConvention.GetValueOrDefault
				(contextCallingConvention.GetValueOrDefault (nativeCallCallingConvention));

			var subject = new DelegateToInteropCompatibleDelegateArgumentPreprocessor (argumentCallingConvention);

			var register = new List<Delegate> ();

			var delegateTypeToDelegateSignatureConverterMock = new Mock<IDelegateTypeToDelegateSignatureConverter> ();

			var delegateSignature = new DelegateSignature (new [] { typeof(int) }, typeof(int), expectedCallingConvention);

			delegateTypeToDelegateSignatureConverterMock.Setup (
				dttdsc => dttdsc.CreateDelegateSignature (typeof(Func<int, int>), expectedCallingConvention))
				.Returns (delegateSignature);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (delegateSignature))
				.Returns (typeof(AddOneDelegate));

			var argumentPreprocessorContext = CreateDummyContext (nativeCallCallingConvention);

			// Customise the context with the stuff we do care about.
			argumentPreprocessorContext = argumentPreprocessorContext.Customise (
				DelegateToInteropCompatibleDelegateArgumentPreprocessor.CreateContextCustomisation (
					delegateTypeToDelegateSignatureConverterMock.Object,
					delegateTypeProviderMock.Object,
					contextCallingConvention,
					register.Add
				)	
			);

			subject.SetContext (argumentPreprocessorContext);

			Func<int, int> addOne = i => i + 1;

			var processedDelegate = (AddOneDelegate) subject.Process (addOne);

			var result = processedDelegate.DynamicInvoke (new object[] { 7 });

			Assert.AreEqual (8, result);

			Assert.IsInstanceOf<AddOneDelegate> (processedDelegate);

			Assert.AreEqual (1, register.Count);

			Assert.AreEqual (processedDelegate, register[0]);
		}

		public delegate int AddOneDelegate(int i);
	}
}

