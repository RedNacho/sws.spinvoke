using System;
using System.Runtime.InteropServices;

using Ninject.Syntax;

namespace Sws.Spinvoke.Ninject.Extensions
{
	public interface ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> : IBindingWhenInNamedWithOrOnSyntax<T>
	{
		ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithCallingConvention(CallingConvention callingConvention);
		ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNonNativeFallback(Func<INonNativeFallbackContext, T> nonNativeFallbackSource);
	}
}

