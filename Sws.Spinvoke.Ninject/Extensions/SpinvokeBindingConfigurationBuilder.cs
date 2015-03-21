using System;
using System.Runtime.InteropServices;

using Ninject;
using Ninject.Planning.Bindings;

namespace Sws.Spinvoke.Ninject.Extensions
{
	internal class SpinvokeBindingConfigurationBuilder<T> : BindingConfigurationBuilder<T>, ISpinvokeBindingWhenInNamedWithOrOnSyntax<T>
	{
		private readonly Action<CallingConvention> _callingConventionCallback;

		private readonly Action<Func<INonNativeFallbackContext, T>> _nonNativeFallbackSourceCallback;

		public SpinvokeBindingConfigurationBuilder (IBindingConfiguration bindingConfiguration, string serviceNames, IKernel kernel, Action<CallingConvention> callingConventionCallback, Action<Func<INonNativeFallbackContext, T>> nonNativeFallbackSourceCallback)
			: base(bindingConfiguration, serviceNames, kernel)
		{
			if (callingConventionCallback == null) {
				throw new ArgumentNullException("callingConventionCallback");
			}

			if (nonNativeFallbackSourceCallback == null) {
				throw new ArgumentNullException ("nonNativeFallbackSourceCallback");
			}

			_callingConventionCallback = callingConventionCallback;
			_nonNativeFallbackSourceCallback = nonNativeFallbackSourceCallback;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithCallingConvention(CallingConvention callingConvention)
		{
			_callingConventionCallback (callingConvention);

			return this;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNonNativeFallback(Func<INonNativeFallbackContext, T> nonNativeFallbackSource)
		{
			_nonNativeFallbackSourceCallback (nonNativeFallbackSource);

			return this;
		}
	}
}

