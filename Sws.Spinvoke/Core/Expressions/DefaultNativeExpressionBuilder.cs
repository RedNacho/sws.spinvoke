using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Expressions
{
	public class DefaultNativeExpressionBuilder : INativeExpressionBuilder
	{
		private readonly INativeDelegateResolver _nativeDelegateResolver;

		private readonly IDelegateTypeToDelegateSignatureConverter _delegateTypeToDelegateSignatureConverter;

		private readonly IDelegateExpressionBuilder _delegateExpressionBuilder;

		public DefaultNativeExpressionBuilder(INativeDelegateResolver nativeDelegateResolver, IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter, IDelegateExpressionBuilder delegateExpressionBuilder)
		{
			if (nativeDelegateResolver == null)
				throw new ArgumentNullException ("nativeDelegateResolver");

			if (delegateTypeToDelegateSignatureConverter == null)
				throw new ArgumentNullException ("delegateTypeToDelegateSignatureConverter");

			if (delegateExpressionBuilder == null)
				throw new ArgumentNullException ("delegateExpressionBuilder");

			_nativeDelegateResolver = nativeDelegateResolver;
			_delegateTypeToDelegateSignatureConverter = delegateTypeToDelegateSignatureConverter;
			_delegateExpressionBuilder = delegateExpressionBuilder;
		}

		public Expression<T> BuildNativeExpression<T> (string libraryName, string functionName, CallingConvention? callingConvention = null, Type explicitDelegateType = null)
		{
			var adaptedDelegateType = typeof(T);

			DelegateSignature delegateSignature;

			if ((!callingConvention.HasValue) && _delegateTypeToDelegateSignatureConverter.HasCallingConvention (adaptedDelegateType)) {
				delegateSignature = _delegateTypeToDelegateSignatureConverter.CreateDelegateSignature (adaptedDelegateType);
			} else if (callingConvention.HasValue) {
				delegateSignature = _delegateTypeToDelegateSignatureConverter.CreateDelegateSignature (adaptedDelegateType, callingConvention.Value);
			} else {
				throw new ArgumentException ("Calling convention cannot be inferred from this delegate type; callingConvention cannot be null.", "callingConvention");
			}

			var nativeDelegateDefinition = new NativeDelegateDefinition (libraryName, functionName, delegateSignature, explicitDelegateType);

			var nativeDelegate = _nativeDelegateResolver.Resolve (nativeDelegateDefinition);

			return _delegateExpressionBuilder.BuildLinqExpression<T> (nativeDelegate);
		}
	}
}

