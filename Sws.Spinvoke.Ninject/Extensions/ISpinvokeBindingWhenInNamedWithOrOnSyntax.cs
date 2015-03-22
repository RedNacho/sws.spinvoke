using System;
using System.Runtime.InteropServices;

using Ninject.Syntax;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject.Extensions
{
	public interface ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> : IBindingWhenInNamedWithOrOnSyntax<T>
	{
		ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithCallingConvention(CallingConvention callingConvention);
		ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNonNativeFallback(Func<NonNativeFallbackContext, T> nonNativeFallbackSource);
		ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNativeDelegateResolver(INativeDelegateResolver nativeDelegateResolver);
		ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNativeDelegateInterceptorFactory(INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory);
	}
}

