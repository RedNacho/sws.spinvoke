using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core
{
	public interface INativeExpressionBuilder
	{
		Expression<T> BuildNativeExpression<T>(string libraryName, string functionName, CallingConvention? callingConvention = null, Type explicitDelegateType = null);
	}
}

