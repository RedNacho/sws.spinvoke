using System;
using System.Linq;
using System.Linq.Expressions;

namespace Sws.Spinvoke.Core.Expressions
{
	public class DefaultDelegateExpressionBuilder : IDelegateExpressionBuilder
	{
		public Expression<T> BuildLinqExpression<T> (Delegate target)
		{
			const string InvokeMethodName = "Invoke";

			if (!typeof(Delegate).IsAssignableFrom(typeof(T))) {
				throw new InvalidOperationException("T must be a delegate type.");
			}

			var targetInvokeMethod = target.GetType ().GetMethod (InvokeMethodName);

			var adaptedInvokeMethod = typeof(T).GetMethod (InvokeMethodName);

			var parameterExpressions = adaptedInvokeMethod.GetParameters ().Select (
				parameterInfo => Expression.Parameter (parameterInfo.ParameterType, parameterInfo.Name)).ToList();

			var targetMethodCall = Expression.Call (
				Expression.Constant (target),
				targetInvokeMethod,
				parameterExpressions);

			return Expression.Lambda<T> (targetMethodCall, parameterExpressions);
		}
	}
}

