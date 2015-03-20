using NUnit.Framework;
using System;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

using Moq;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.Interception.ReturnPostprocessing;
using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class NativeDelegateInterceptorTests
	{
		private const string LibraryName = "Test";

		private readonly CallingConvention CallingConvention = CallingConvention.Cdecl;

		private Mock<INativeDelegateResolver> _nativeDelegateResolverMock;

		private NativeDelegateInterceptor _subject;

		[TestFixtureSetUp ()]
		public void TestSetup()
		{
			_nativeDelegateResolverMock = new Mock<INativeDelegateResolver> ();

			_subject = new NativeDelegateInterceptor (LibraryName, CallingConvention, _nativeDelegateResolverMock.Object);
		}

		[Test ()]
		public void InterceptorCreatesDelegateAndInvokesIt ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y;
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTest).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				null
			), Times.Once());

			Assert.AreEqual (1, addCalls.Count);

			Assert.AreEqual (Tuple.Create (X, Y), addCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
		}

		[Test ()]
		public void InterceptorAllowsRepeatCallsForSameMethod ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			const int X2 = 7;
			const int Y2 = 9;
			const int X2PlusY2 = 16;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y;
				}));

			var methodInfo = typeof(IInterceptorTest).GetMethod ("Add");

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (methodInfo);
			invocationMock.SetupProperty (i => i.ReturnValue);

			var invocationMock2 = new Mock<IInvocation> ();

			invocationMock2.SetupGet (i => i.Arguments).Returns (new object[] { X2, Y2 });
			invocationMock2.SetupGet (i => i.Method).Returns (methodInfo);
			invocationMock2.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			_subject.Intercept (invocationMock2.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				null
			), Times.Exactly(2));

			Assert.AreEqual (2, addCalls.Count);

			Assert.AreEqual (Tuple.Create (X, Y), addCalls.First ());
			Assert.AreEqual (Tuple.Create (X2, Y2), addCalls.Skip (1).First ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
			Assert.AreEqual (X2PlusY2, invocationMock2.Object.ReturnValue);
		}

		[Test ()]
		public void InterceptorCreatesModifiedDelegateAndInvokesItIfExplicitDelegateTypeSet ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y;
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithExplicitDelegateType).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				typeof(AddDelegate)
			), Times.Once());

			Assert.AreEqual (1, addCalls.Count);

			Assert.AreEqual (Tuple.Create (X, Y), addCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
		}

		[Test ()]
		public void InterceptorCreatesModifiedDelegateAndInvokesItIfTypesAndCallingConventionOverridden ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<decimal, decimal>> addCalls = new List<Tuple<decimal, decimal>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDecimalDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y;
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithDifferentTypes).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(decimal), typeof(decimal) }, typeof(decimal), CallingConvention.FastCall),
				null
			), Times.Once());

			Assert.AreEqual (1, addCalls.Count);

			Assert.AreEqual (Tuple.Create ((decimal)X, (decimal)Y), addCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
		}

		[Test ()]
		public void InterceptorCreatesModifiedDelegateAndInvokesItIfLibraryAndFunctionNamesOverridden ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y;
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithDifferentLibraryAndFunctionNames).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				"ExplicitTestLibrary",
				"ExplicitTestFunction",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				null
			), Times.Once());

			Assert.AreEqual (1, addCalls.Count);

			Assert.AreEqual (Tuple.Create (X, Y), addCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
		}

		[Test ()]
		public void InterceptorModifiesOutputIfReturnPostprocessorSpecified ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y - 1;
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithReturnPostprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				null
			), Times.Once());

			Assert.AreEqual (1, addCalls.Count);

			Assert.AreEqual (Tuple.Create (X, Y), addCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
		}

		[Test ()]
		public void InterceptorModifiesInputIfArgumentPreprocessorSpecified ()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return (x + 1) + (y + 1);
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithArgumentPreprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				null
			), Times.Once());

			Assert.AreEqual (1, addCalls.Count);

			Assert.AreEqual (Tuple.Create (X - 1, Y - 1), addCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
		}

		private void VerifyNativeDelegateResolverResolveCall(NativeDelegateDefinition nativeDelegateDefinition, Times times)
		{
			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.Is<NativeDelegateDefinition> (
				ndd => ndd.FileName == nativeDelegateDefinition.FileName)), times);

			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.Is<NativeDelegateDefinition> (
				ndd => ndd.FunctionName == nativeDelegateDefinition.FunctionName)), times);

			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.Is<NativeDelegateDefinition> (
				ndd => ndd.DelegateSignature.CallingConvention == nativeDelegateDefinition.DelegateSignature.CallingConvention)), times);

			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.Is<NativeDelegateDefinition> (
				ndd => ndd.DelegateSignature.InputTypes.SequenceEqual(nativeDelegateDefinition.DelegateSignature.InputTypes))), times);

			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.Is<NativeDelegateDefinition> (
				ndd => ndd.DelegateSignature.OutputType == nativeDelegateDefinition.DelegateSignature.OutputType)), times);

			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.Is<NativeDelegateDefinition> (
				ndd => ndd.ExplicitDelegateType == nativeDelegateDefinition.ExplicitDelegateType)), times);

			_nativeDelegateResolverMock.Verify (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()), times);
		}
	}

	public delegate int AddDelegate(int x, int y);

	public delegate decimal AddDecimalDelegate(decimal x, decimal y);

	public interface IInterceptorTest
	{
		int Add(int x, int y);
	}

	public interface IInterceptorTestWithReturnPostprocessor
	{
		[return: NativeReturnsOneLess()]
		int Add(int x, int y);
	}

	public interface IInterceptorTestWithArgumentPreprocessor
	{
		int Add([NativeArgumentOneLessAttribute] int x, [NativeArgumentOneLessAttribute] int y);
	}

	public interface IInterceptorTestWithDifferentTypes
	{
		[NativeDelegateDefinitionOverride(OutputType = typeof(decimal), InputTypes = new [] { typeof(decimal), typeof(decimal) }, CallingConvention = CallingConvention.FastCall)]
		int Add(int x, int y);
	}

	public interface IInterceptorTestWithDifferentLibraryAndFunctionNames
	{
		[NativeDelegateDefinitionOverride(LibraryName = "ExplicitTestLibrary", FunctionName = "ExplicitTestFunction")]
		int Add(int x, int y);
	}

	public interface IInterceptorTestWithExplicitDelegateType
	{
		[NativeDelegateDefinitionOverride(ExplicitDelegateType = typeof(AddDelegate))]
		int Add(int x, int y);
	}

	public class NativeReturnsOneLessAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsOneLessAttribute()
			: base(CreateAddOneReturnPostprocessor())
		{
		}

		private static IReturnPostprocessor CreateAddOneReturnPostprocessor()
		{
			var returnPostprocessorMock = new Mock<IReturnPostprocessor> ();

			returnPostprocessorMock.Setup (rp => rp.CanProcess (It.IsAny<object> (), It.IsAny<Type> ()))
				.Returns ((object output, Type requiredType) =>
					output is int && requiredType == typeof(int));

			returnPostprocessorMock.Setup (rp => rp.Process(It.IsAny<object>(), It.IsAny<Type>()))
				.Returns((object output, Type requiredType) =>
					(int)((int)output + 1));

			return returnPostprocessorMock.Object;
		}
	}

	public class NativeArgumentOneLessAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentOneLessAttribute()
			: base(CreateSubtractOneArgumentPreprocessor())
		{
		}

		private static IArgumentPreprocessor CreateSubtractOneArgumentPreprocessor()
		{
			var argumentPreprocessorMock = new Mock<IArgumentPreprocessor> ();

			argumentPreprocessorMock.Setup (ap => ap.CanProcess(It.IsAny<object>()))
				.Returns((object input) => input is int);

			argumentPreprocessorMock.Setup (ap => ap.Process(It.IsAny<object>()))
				.Returns((object input) => (int)input - 1);

			argumentPreprocessorMock.Setup (ap => ap.ReleaseProcessedInput(It.IsAny<object>()))
				.Callback(() => { });

			return argumentPreprocessorMock.Object;
		}
	}
}

