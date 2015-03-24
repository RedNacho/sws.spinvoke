using System;
using System.Linq.Expressions;

namespace Sws.Spinvoke.Core.Expressions
{
	public interface IDelegateExpressionBuilder
	{
		Expression<T> BuildLinqExpression<T>(Delegate target);
	}
}

