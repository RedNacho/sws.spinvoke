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
		public void InterceptorProceedsWithInvocationOnTargetIfMapNativeFalse()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			List<Tuple<int, int>> addCalls = new List<Tuple<int, int>> ();

			List<Tuple<int, int>> fallbackAddCalls = new List<Tuple<int, int>> ();

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => {
					addCalls.Add (Tuple.Create (x, y));
					return x + y;
				}));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithUnmappedMethod).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);
			invocationMock.Setup (i => i.Proceed())
				.Callback(() => { 
					fallbackAddCalls.Add(Tuple.Create(X, Y));
					invocationMock.Object.ReturnValue = XPlusY;
				});

			_subject.Intercept (invocationMock.Object);

			VerifyNativeDelegateResolverResolveCall (new NativeDelegateDefinition (
				LibraryName,
				"Add",
				new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention),
				null
			), Times.Never());

			Assert.AreEqual (0, addCalls.Count);

			Assert.AreEqual (1, fallbackAddCalls.Count);

			Assert.AreEqual (Tuple.Create (X, Y), fallbackAddCalls.Single ());

			Assert.AreEqual (XPlusY, invocationMock.Object.ReturnValue);
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

		[Test ()]
		public void InterceptorReleasesModifiedInputIfArgumentPreprocessorSpecified ()
		{
			const int X = 2;
			const int Y = 3;

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => (x + 1) + (y + 1)));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithArgumentPreprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			NativeArgumentOneLessAttribute.ReleaseProcessedInputCalls.Clear ();

			_subject.Intercept (invocationMock.Object);

			Assert.AreEqual (2, NativeArgumentOneLessAttribute.ReleaseProcessedInputCalls.Count);

			Assert.AreEqual (X - 1, NativeArgumentOneLessAttribute.ReleaseProcessedInputCalls [0]);
			Assert.AreEqual (Y - 1, NativeArgumentOneLessAttribute.ReleaseProcessedInputCalls [1]);
		}

		[Test ()]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InterceptorThrowsExceptionIfArgumentPreprocessorCannotProcessArgument ()
		{
			const int X = 2;
			const int Y = 3;

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => (x + 1) + (y + 1)));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { (decimal) X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithArgumentPreprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);
		}

		[Test ()]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InterceptorThrowsExceptionIfReturnPostprocessorCannotProcessReturnValue ()
		{
			const int X = 2;
			const int Y = 3;

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => (Func<int, int, decimal>)((x, y) => (decimal)(x + y - 1)));

			var invocationMock = new Mock<IInvocation> ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithReturnPostprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);

			_subject.Intercept (invocationMock.Object);
		}

		[Test ()]
		public void InterceptorInvokesSetContextForContextualArgumentPreprocessor()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => x + y));

			var invocationMock = new Mock<IInvocation> ();

			var proxy = new object ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithContextualArgumentPreprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);
			invocationMock.SetupGet (i => i.Proxy).Returns(proxy);

			var subject = new NativeDelegateInterceptor (LibraryName, CallingConvention, _nativeDelegateResolverMock.Object);

			ContextualArgumentPreprocessorMockAttribute.CanProcessContexts.Clear ();
			ContextualArgumentPreprocessorMockAttribute.ProcessContexts.Clear ();
			ContextualArgumentPreprocessorMockAttribute.ReleaseProcessedInputContexts.Clear ();

			subject.Intercept (invocationMock.Object);

			Assert.AreEqual(2, ContextualArgumentPreprocessorMockAttribute.CanProcessContexts.Count);
			Assert.AreEqual(2, ContextualArgumentPreprocessorMockAttribute.ProcessContexts.Count);
			Assert.AreEqual (2, ContextualArgumentPreprocessorMockAttribute.ReleaseProcessedInputContexts.Count);

			var arg0canProcessContext = ContextualArgumentPreprocessorMockAttribute.CanProcessContexts.FirstOrDefault (tuple => tuple.Item1 != null && tuple.Item1.ArgumentIndex == 0);
			var arg0processContext = ContextualArgumentPreprocessorMockAttribute.ProcessContexts.FirstOrDefault (tuple => tuple.Item1 != null && tuple.Item1.ArgumentIndex == 0);
			var arg0releaseProcessedInputContext = ContextualArgumentPreprocessorMockAttribute.ReleaseProcessedInputContexts.FirstOrDefault (tuple => tuple.Item1 != null && tuple.Item1.ArgumentIndex == 0);

			var arg1canProcessContext = ContextualArgumentPreprocessorMockAttribute.CanProcessContexts.FirstOrDefault (tuple => tuple.Item1 != null && tuple.Item1.ArgumentIndex == 1);
			var arg1processContext = ContextualArgumentPreprocessorMockAttribute.ProcessContexts.FirstOrDefault (tuple => tuple.Item1 != null && tuple.Item1.ArgumentIndex == 1);
			var arg1releaseProcessedInputContext = ContextualArgumentPreprocessorMockAttribute.ReleaseProcessedInputContexts.FirstOrDefault (tuple => tuple.Item1 != null && tuple.Item1.ArgumentIndex == 1);

			Assert.IsNotNull (arg0canProcessContext);
			Assert.IsNotNull (arg0processContext);
			Assert.IsNotNull (arg0releaseProcessedInputContext);

			Assert.AreEqual (X, arg0canProcessContext.Item2);
			Assert.AreEqual (X, arg0processContext.Item2);
			Assert.AreEqual (X, arg0releaseProcessedInputContext.Item2);

			Assert.AreEqual (arg0processContext.Item1, arg0canProcessContext.Item1);
			Assert.AreEqual (arg0processContext.Item1, arg0releaseProcessedInputContext.Item1);

			var arg0context = arg0processContext.Item1;

			Assert.IsNotNull (arg1canProcessContext);
			Assert.IsNotNull (arg1processContext);
			Assert.IsNotNull (arg1releaseProcessedInputContext);

			Assert.AreEqual (Y, arg1canProcessContext.Item2);
			Assert.AreEqual (Y, arg1processContext.Item2);
			Assert.AreEqual (Y, arg1releaseProcessedInputContext.Item2);

			Assert.AreEqual (arg1processContext.Item1, arg1canProcessContext.Item1);
			Assert.AreEqual (arg1processContext.Item1, arg1releaseProcessedInputContext.Item1);

			var arg1context = arg0processContext.Item1;

			foreach (var argContext in new [] { arg0context, arg1context }) {
				Assert.AreEqual (invocationMock.Object, argContext.Invocation);
				Assert.AreEqual ("Add", argContext.NativeDelegateMapping.FunctionName);
				Assert.AreEqual (LibraryName, argContext.NativeDelegateMapping.LibraryName);
				Assert.AreEqual (CallingConvention, argContext.NativeDelegateMapping.CallingConvention);
				Assert.IsNull (argContext.NativeDelegateMapping.ExplicitDelegateType);
				Assert.AreEqual (2, argContext.NativeDelegateMapping.ArgumentPreprocessors.Length);
				Assert.IsNotNull (argContext.NativeDelegateMapping.ArgumentPreprocessors [0] as IContextualArgumentPreprocessor);
				Assert.IsNotNull (argContext.NativeDelegateMapping.ArgumentPreprocessors [1] as IContextualArgumentPreprocessor);
				Assert.IsNotNull (argContext.NativeDelegateMapping.ReturnPostprocessor as ChangeTypeReturnPostprocessor);
				Assert.AreEqual (2, argContext.NativeDelegateMapping.InputTypes.Length);
				Assert.AreEqual (typeof(int), argContext.NativeDelegateMapping.InputTypes[0]);
				Assert.AreEqual (typeof(int), argContext.NativeDelegateMapping.InputTypes[1]);
				Assert.AreEqual (typeof(int), argContext.NativeDelegateMapping.OutputType);
				Assert.IsTrue (argContext.NativeDelegateMapping.MapNative);
			}
		}

		[Test ()]
		public void InterceptorInvokesSetContextForContextualReturnPostprocessor()
		{
			const int X = 2;
			const int Y = 3;
			const int XPlusY = 5;

			_nativeDelegateResolverMock.ResetCalls ();

			_nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (() => new AddDelegate((x, y) => x + y));

			var invocationMock = new Mock<IInvocation> ();

			var proxy = new object ();

			invocationMock.SetupGet (i => i.Arguments).Returns (new object[] { X, Y });
			invocationMock.SetupGet (i => i.Method).Returns (typeof(IInterceptorTestWithContextualReturnPostprocessor).GetMethod ("Add"));
			invocationMock.SetupProperty (i => i.ReturnValue);
			invocationMock.SetupGet (i => i.Proxy).Returns(proxy);

			var subject = new NativeDelegateInterceptor (LibraryName, CallingConvention, _nativeDelegateResolverMock.Object);

			ContextualReturnPostprocessorMockAttribute.CanProcessContexts.Clear ();
			ContextualReturnPostprocessorMockAttribute.ProcessContexts.Clear ();

			subject.Intercept (invocationMock.Object);

			Assert.AreEqual(1, ContextualReturnPostprocessorMockAttribute.CanProcessContexts.Count);
			Assert.AreEqual(1, ContextualReturnPostprocessorMockAttribute.ProcessContexts.Count);

			var returnCanProcessContext = ContextualReturnPostprocessorMockAttribute.CanProcessContexts.Single();
			var returnProcessContext = ContextualReturnPostprocessorMockAttribute.ProcessContexts.Single();

			Assert.IsNotNull (returnCanProcessContext);
			Assert.IsNotNull (returnProcessContext);

			Assert.AreEqual (XPlusY, returnCanProcessContext.Item2);
			Assert.AreEqual (typeof(int), returnCanProcessContext.Item3);
			Assert.AreEqual (XPlusY, returnProcessContext.Item2);
			Assert.AreEqual (typeof(int), returnProcessContext.Item3);

			Assert.AreEqual (returnProcessContext.Item1, returnCanProcessContext.Item1);

			var returnContext = returnProcessContext.Item1;

			Assert.IsNotNull (returnContext);

			Assert.AreEqual (invocationMock.Object, returnContext.Invocation);
			Assert.AreEqual ("Add", returnContext.NativeDelegateMapping.FunctionName);
			Assert.AreEqual (LibraryName, returnContext.NativeDelegateMapping.LibraryName);
			Assert.AreEqual (CallingConvention, returnContext.NativeDelegateMapping.CallingConvention);
			Assert.IsNull (returnContext.NativeDelegateMapping.ExplicitDelegateType);
			Assert.AreEqual (2, returnContext.NativeDelegateMapping.ArgumentPreprocessors.Length);
			Assert.IsNotNull (returnContext.NativeDelegateMapping.ArgumentPreprocessors [0] as ChangeTypeArgumentPreprocessor);
			Assert.IsNotNull (returnContext.NativeDelegateMapping.ArgumentPreprocessors [1] as ChangeTypeArgumentPreprocessor);
			Assert.IsNotNull (returnContext.NativeDelegateMapping.ReturnPostprocessor as IContextualReturnPostprocessor);
			Assert.AreEqual (2, returnContext.NativeDelegateMapping.InputTypes.Length);
			Assert.AreEqual (typeof(int), returnContext.NativeDelegateMapping.InputTypes[0]);
			Assert.AreEqual (typeof(int), returnContext.NativeDelegateMapping.InputTypes[1]);
			Assert.AreEqual (typeof(int), returnContext.NativeDelegateMapping.OutputType);
			Assert.IsTrue (returnContext.NativeDelegateMapping.MapNative);
			Assert.AreEqual (2, returnContext.ProcessedArguments.Length);
			Assert.AreEqual (X, returnContext.ProcessedArguments [0]);
			Assert.AreEqual (Y, returnContext.ProcessedArguments [1]);
			Assert.AreEqual (2, returnContext.DelegateSignature.InputTypes.Length);
			Assert.AreEqual (typeof(int), returnContext.DelegateSignature.InputTypes [0]);
			Assert.AreEqual (typeof(int), returnContext.DelegateSignature.InputTypes [1]);
			Assert.AreEqual (typeof(int), returnContext.DelegateSignature.OutputType);
			Assert.AreEqual (CallingConvention, returnContext.DelegateSignature.CallingConvention);
			Assert.IsNotNull (returnContext.DelegateInstance as AddDelegate);
			Assert.AreEqual (_nativeDelegateResolverMock.Object, returnContext.NativeDelegateResolver);
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

	public interface IInterceptorTestWithUnmappedMethod
	{
		[NativeDelegateDefinitionOverride(MapNative = false)]
		int Add(int x, int y);
	}

	public interface IInterceptorTestWithContextualArgumentPreprocessor
	{
		int Add([ContextualArgumentPreprocessorMock] int x, [ContextualArgumentPreprocessorMock] int y);
	}

	public interface IInterceptorTestWithContextualReturnPostprocessor
	{
		[return: ContextualReturnPostprocessorMock] int Add(int x, int y);
	}

	public class ContextualReturnPostprocessorMockAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public ContextualReturnPostprocessorMockAttribute()
			: base(CreateContextualReturnPostprocessorMock())
		{
		}

		public static IList<Tuple<ReturnPostprocessorContext, object, Type>> CanProcessContexts = new List<Tuple<ReturnPostprocessorContext, object, Type>> ();
		public static IList<Tuple<ReturnPostprocessorContext, object, Type>> ProcessContexts = new List<Tuple<ReturnPostprocessorContext, object, Type>> ();

		private static IContextualReturnPostprocessor CreateContextualReturnPostprocessorMock ()
		{
			ReturnPostprocessorContext currentContext = null;

			var returnPostprocessorMock = new Mock<IContextualReturnPostprocessor> ();

			returnPostprocessorMock.Setup (rp => rp.CanProcess (It.IsAny<object> (), It.IsAny<Type> ()))
				.Callback ((object obj, Type type) => CanProcessContexts.Add(Tuple.Create(currentContext, obj, type)))
				.Returns (true);

			returnPostprocessorMock.Setup (rp => rp.Process (It.IsAny<object> (), It.IsAny<Type> ()))
				.Callback ((object obj, Type type) => ProcessContexts.Add(Tuple.Create(currentContext, obj, type)))
				.Returns ((object obj, Type type) => obj);

			returnPostprocessorMock.Setup (rp => rp.SetContext (It.IsAny<ReturnPostprocessorContext> ()))
				.Callback ((ReturnPostprocessorContext context) => currentContext = context);

			return returnPostprocessorMock.Object;
		}
	}

	public class ContextualArgumentPreprocessorMockAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public ContextualArgumentPreprocessorMockAttribute()
			: base(CreateContextualArgumentPreprocessorMock())
		{
		}

		public static IList<Tuple<ArgumentPreprocessorContext, object>> ProcessContexts = new List<Tuple<ArgumentPreprocessorContext, object>>();
		public static IList<Tuple<ArgumentPreprocessorContext, object>> CanProcessContexts = new List<Tuple<ArgumentPreprocessorContext, object>>();
		public static IList<Tuple<ArgumentPreprocessorContext, object>> ReleaseProcessedInputContexts = new List<Tuple<ArgumentPreprocessorContext, object>>();

		private static IContextualArgumentPreprocessor CreateContextualArgumentPreprocessorMock()
		{
			ArgumentPreprocessorContext currentContext = null;

			var argumentPreprocessorMock = new Mock<IContextualArgumentPreprocessor> ();

			argumentPreprocessorMock.Setup (ap => ap.CanProcess (It.IsAny<object> ()))
				.Callback ((object obj) => CanProcessContexts.Add (Tuple.Create (currentContext, obj)))
				.Returns (true);

			argumentPreprocessorMock.Setup (ap => ap.Process (It.IsAny<object>()))
				.Callback ((object obj) => ProcessContexts.Add (Tuple.Create (currentContext, obj)))
				.Returns((object obj) => obj);

			argumentPreprocessorMock.Setup (ap => ap.ReleaseProcessedInput (It.IsAny<object> ()))
				.Callback ((object obj) => ReleaseProcessedInputContexts.Add (Tuple.Create (currentContext, obj)));

			argumentPreprocessorMock.Setup (ap => ap.SetContext (It.IsAny<ArgumentPreprocessorContext> ()))
				.Callback ((ArgumentPreprocessorContext context) => currentContext = context);

			return argumentPreprocessorMock.Object;
		}
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

		public static IList<int> ReleaseProcessedInputCalls = new List<int> ();

		private static IArgumentPreprocessor CreateSubtractOneArgumentPreprocessor()
		{
			var argumentPreprocessorMock = new Mock<IArgumentPreprocessor> ();

			argumentPreprocessorMock.Setup (ap => ap.CanProcess(It.IsAny<object>()))
				.Returns((object input) => input is int);

			argumentPreprocessorMock.Setup (ap => ap.Process(It.IsAny<object>()))
				.Returns((object input) => (int)input - 1);

			argumentPreprocessorMock.Setup (ap => ap.ReleaseProcessedInput(It.IsAny<object>()))
				.Callback((object input) => ReleaseProcessedInputCalls.Add((int)input));

			return argumentPreprocessorMock.Object;
		}
	}
}

