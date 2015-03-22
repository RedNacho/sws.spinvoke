using System;
using System.Runtime.InteropServices;

using Ninject;
using Ninject.Planning.Bindings;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject.Extensions
{
	internal class SpinvokeBindingConfigurationBuilder<T> : BindingConfigurationBuilder<T>, ISpinvokeBindingWhenInNamedWithOrOnSyntax<T>
	{
		private readonly Action<CallingConvention> _callingConventionCallback;

		private readonly Action<Func<NonNativeFallbackContext, T>> _nonNativeFallbackSourceCallback;

		private readonly Action<INativeDelegateResolver> _nativeDelegateResolverCallback;

		private readonly Action<INativeDelegateInterceptorFactory> _nativeDelegateInterceptorFactoryCallback;

		public SpinvokeBindingConfigurationBuilder (IBindingConfiguration bindingConfiguration, string serviceNames, IKernel kernel, Action<CallingConvention> callingConventionCallback, Action<Func<NonNativeFallbackContext, T>> nonNativeFallbackSourceCallback, Action<INativeDelegateResolver> nativeDelegateResolverCallback, Action<INativeDelegateInterceptorFactory> nativeDelegateInterceptorFactoryCallback)
			: base(bindingConfiguration, serviceNames, kernel)
		{
			if (callingConventionCallback == null) {
				throw new ArgumentNullException("callingConventionCallback");
			}

			if (nonNativeFallbackSourceCallback == null) {
				throw new ArgumentNullException ("nonNativeFallbackSourceCallback");
			}

			if (nativeDelegateResolverCallback == null) {
				throw new ArgumentNullException ("nativeDelegateResolverCallback");
			}

			if (nativeDelegateInterceptorFactoryCallback == null) {
				throw new ArgumentNullException ("nativeDelegateInterceptorFactoryCallback");
			}

			_callingConventionCallback = callingConventionCallback;
			_nonNativeFallbackSourceCallback = nonNativeFallbackSourceCallback;
			_nativeDelegateResolverCallback = nativeDelegateResolverCallback;
			_nativeDelegateInterceptorFactoryCallback = nativeDelegateInterceptorFactoryCallback;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithCallingConvention(CallingConvention callingConvention)
		{
			_callingConventionCallback (callingConvention);

			return this;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNonNativeFallback(Func<NonNativeFallbackContext, T> nonNativeFallbackSource)
		{
			if (nonNativeFallbackSource == null)
				throw new ArgumentNullException ("nonNativeFallbackSource");

			_nonNativeFallbackSourceCallback (nonNativeFallbackSource);

			return this;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNativeDelegateResolver (INativeDelegateResolver nativeDelegateResolver)
		{
			if (nativeDelegateResolver == null)
				throw new ArgumentNullException ("nativeDelegateResolver");

			_nativeDelegateResolverCallback (nativeDelegateResolver);

			return this;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithNativeDelegateInterceptorFactory (INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory)
		{
			if (nativeDelegateInterceptorFactory == null)
				throw new ArgumentNullException ("nativeDelegateInterceptorFactory");

			_nativeDelegateInterceptorFactoryCallback (nativeDelegateInterceptorFactory);

			return this;
		}
	}
}

