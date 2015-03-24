using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

using Moq;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.Expressions;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DefaultNativeExpressionBuilderTests
	{
		[Test ()]
		public void BuildNativeExpressionCreatesExpressionFromCollaboratorsWithExplicitCallingConvention ()
		{
			const string LibraryName = "TestLibrary";
			const string FunctionName = "TestFunction";
			const CallingConvention CallingConvention = CallingConvention.Cdecl;

			var explicitDelegateType = typeof(Action<int, int>);

			var nativeDelegateResolverMock = new Mock<INativeDelegateResolver> ();

			Action nativeAction = () => { };

			var expressionCalled = false;

			Action expressionCalledSetter = () => expressionCalled = true;

			var expression = Expression.Lambda<Action>(
				Expression.Call(Expression.Constant(expressionCalledSetter),
					expressionCalledSetter.GetType().GetMethod("Invoke")));

			nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (nativeAction);

			var delegateTypeToDelegateSignatureMock = new Mock<IDelegateTypeToDelegateSignatureConverter> ();

			var delegateSignature = new DelegateSignature (new Type[0], typeof(void), System.Runtime.InteropServices.CallingConvention.Cdecl);

			delegateTypeToDelegateSignatureMock.Setup(dttds => dttds.HasCallingConvention(It.IsAny<Type>()))
				.Returns(false);

			delegateTypeToDelegateSignatureMock.Setup (dttds => dttds.CreateDelegateSignature (It.IsAny<Type> (), It.IsAny<CallingConvention> ()))
				.Returns (delegateSignature);

			var delegateExpressionBuilderMock = new Mock<IDelegateExpressionBuilder> ();

			delegateExpressionBuilderMock.Setup(deb => deb.BuildLinqExpression<Action>(It.IsAny<Delegate>()))
				.Returns(expression);

			var subject = new DefaultNativeExpressionBuilder (nativeDelegateResolverMock.Object, delegateTypeToDelegateSignatureMock.Object, delegateExpressionBuilderMock.Object);

			var nativeExpression = subject.BuildNativeExpression<Action> (LibraryName, FunctionName, CallingConvention, explicitDelegateType);

			nativeExpression.Compile () ();

			Assert.IsTrue (expressionCalled);

			delegateTypeToDelegateSignatureMock.Verify (dttds => dttds.CreateDelegateSignature (typeof(Action), CallingConvention), Times.Once);

			delegateTypeToDelegateSignatureMock.Verify (dttds => dttds.CreateDelegateSignature (It.IsAny<Type>(), It.IsAny<CallingConvention>()), Times.Once);

			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.DelegateSignature == delegateSignature)), Times.Once);
			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.ExplicitDelegateType == explicitDelegateType)), Times.Once);
			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.FileName == LibraryName)), Times.Once);
			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.FunctionName == FunctionName)), Times.Once);

			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.IsAny<NativeDelegateDefinition>()), Times.Once);

			delegateExpressionBuilderMock.Verify (deb => deb.BuildLinqExpression<Action> (nativeAction), Times.Once);

			delegateExpressionBuilderMock.Verify (deb => deb.BuildLinqExpression<Action> (It.IsAny<Action> ()), Times.Once);
		}

		[Test ()]
		public void BuildNativeExpressionCreatesExpressionFromCollaboratorsWithImplicitCallingConvention ()
		{
			const string LibraryName = "TestLibrary";
			const string FunctionName = "TestFunction";

			var explicitDelegateType = typeof(Action<int, int>);

			var nativeDelegateResolverMock = new Mock<INativeDelegateResolver> ();

			Action nativeAction = () => { };

			var expressionCalled = false;

			Action expressionCalledSetter = () => expressionCalled = true;

			var expression = Expression.Lambda<Action>(
				Expression.Call(Expression.Constant(expressionCalledSetter),
					expressionCalledSetter.GetType().GetMethod("Invoke")));

			nativeDelegateResolverMock.Setup (ndr => ndr.Resolve (It.IsAny<NativeDelegateDefinition> ()))
				.Returns (nativeAction);

			var delegateTypeToDelegateSignatureMock = new Mock<IDelegateTypeToDelegateSignatureConverter> ();

			var delegateSignature = new DelegateSignature (new Type[0], typeof(void), System.Runtime.InteropServices.CallingConvention.Cdecl);

			delegateTypeToDelegateSignatureMock.Setup (dttds => dttds.CreateDelegateSignature (It.IsAny<Type> ()))
				.Returns (delegateSignature);

			var delegateExpressionBuilderMock = new Mock<IDelegateExpressionBuilder> ();

			delegateTypeToDelegateSignatureMock.Setup(dttds => dttds.HasCallingConvention(It.IsAny<Type>()))
				.Returns(true);

			delegateExpressionBuilderMock.Setup(deb => deb.BuildLinqExpression<Action>(It.IsAny<Delegate>()))
				.Returns(expression);

			var subject = new DefaultNativeExpressionBuilder (nativeDelegateResolverMock.Object, delegateTypeToDelegateSignatureMock.Object, delegateExpressionBuilderMock.Object);

			var nativeExpression = subject.BuildNativeExpression<Action> (LibraryName, FunctionName, null, explicitDelegateType);

			nativeExpression.Compile () ();

			Assert.IsTrue (expressionCalled);

			delegateTypeToDelegateSignatureMock.Verify (dttds => dttds.CreateDelegateSignature (typeof(Action)), Times.Once);

			delegateTypeToDelegateSignatureMock.Verify (dttds => dttds.CreateDelegateSignature (It.IsAny<Type>()), Times.Once);

			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.DelegateSignature == delegateSignature)), Times.Once);
			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.ExplicitDelegateType == explicitDelegateType)), Times.Once);
			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.FileName == LibraryName)), Times.Once);
			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.Is<NativeDelegateDefinition>(ndd => ndd.FunctionName == FunctionName)), Times.Once);

			nativeDelegateResolverMock.Verify(ndr => ndr.Resolve(It.IsAny<NativeDelegateDefinition>()), Times.Once);

			delegateExpressionBuilderMock.Verify (deb => deb.BuildLinqExpression<Action> (nativeAction), Times.Once);

			delegateExpressionBuilderMock.Verify (deb => deb.BuildLinqExpression<Action> (It.IsAny<Action> ()), Times.Once);
		}

		[Test ()]
		[ExpectedException (typeof(ArgumentException))]
		public void BuildNativeExpressionThrowsExceptionIfCallingConventionRequiredButNotSupplied ()
		{
			const string LibraryName = "TestLibrary";
			const string FunctionName = "TestFunction";

			var explicitDelegateType = typeof(Action<int, int>);

			var delegateTypeToDelegateSignatureMock = new Mock<IDelegateTypeToDelegateSignatureConverter> ();

			delegateTypeToDelegateSignatureMock.Setup(dttds => dttds.HasCallingConvention(It.IsAny<Type>()))
				.Returns(false);

			var subject = new DefaultNativeExpressionBuilder (Mock.Of<INativeDelegateResolver>(), delegateTypeToDelegateSignatureMock.Object, Mock.Of<IDelegateExpressionBuilder>());

			subject.BuildNativeExpression<Action> (LibraryName, FunctionName, null, explicitDelegateType);
		}
	}
}

