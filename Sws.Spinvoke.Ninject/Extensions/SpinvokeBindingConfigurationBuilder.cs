using System;
using System.Runtime.InteropServices;

using Ninject;
using Ninject.Planning.Bindings;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.Ninject.Providers;
using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Ninject.Extensions
{
	internal class SpinvokeBindingConfigurationBuilder<T> : BindingConfigurationBuilder<T>, ISpinvokeBindingWhenInNamedWithOrOnSyntax<T>
	{
		private readonly Action<CallingConvention> _callingConventionCallback;

		private readonly Action<Func<NonNativeFallbackContext, T>> _nonNativeFallbackSourceCallback;

		private readonly Action<INativeDelegateResolver> _nativeDelegateResolverCallback;

		private readonly Action<INativeDelegateInterceptorFactory> _nativeDelegateInterceptorFactoryCallback;

		private readonly Action<IProxyGenerator> _proxyGeneratorCallback;

		private readonly Action<Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext>> _argumentPreprocessorContextDecoratorCallback;

		private readonly Action<Func<ReturnPostprocessorContext, ReturnPostprocessorContext>> _returnPostprocessorContextDecoratorCallback;

		public SpinvokeBindingConfigurationBuilder (IBindingConfiguration bindingConfiguration,
			string serviceNames,
			IKernel kernel,
			Action<CallingConvention> callingConventionCallback,
			Action<Func<NonNativeFallbackContext, T>> nonNativeFallbackSourceCallback,
			Action<INativeDelegateResolver> nativeDelegateResolverCallback,
			Action<INativeDelegateInterceptorFactory> nativeDelegateInterceptorFactoryCallback,
			Action<IProxyGenerator> proxyGeneratorCallback,
			Action<Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext>> argumentPreprocessorContextDecoratorCallback,
			Action<Func<ReturnPostprocessorContext, ReturnPostprocessorContext>> returnPostprocessorContextDecoratorCallback)
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

			if (proxyGeneratorCallback == null) {
				throw new ArgumentNullException ("proxyGeneratorCallback");
			}

			if (argumentPreprocessorContextDecoratorCallback == null) {
				throw new ArgumentNullException ("argumentPreprocessorContextDecoratorCallback");
			}

			if (returnPostprocessorContextDecoratorCallback == null) {
				throw new ArgumentNullException ("returnPostprocessorContextDecoratorCallback");
			}

			_callingConventionCallback = callingConventionCallback;
			_nonNativeFallbackSourceCallback = nonNativeFallbackSourceCallback;
			_nativeDelegateResolverCallback = nativeDelegateResolverCallback;
			_nativeDelegateInterceptorFactoryCallback = nativeDelegateInterceptorFactoryCallback;
			_proxyGeneratorCallback = proxyGeneratorCallback;
			_argumentPreprocessorContextDecoratorCallback = argumentPreprocessorContextDecoratorCallback;
			_returnPostprocessorContextDecoratorCallback = returnPostprocessorContextDecoratorCallback;
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

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithProxyGenerator (IProxyGenerator proxyGenerator)
		{
			if (proxyGenerator == null)
				throw new ArgumentNullException ("proxyGenerator");

			_proxyGeneratorCallback (proxyGenerator);

			return this;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithArgumentPreprocessorContextDecorator (
			Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext> decorator)
		{
			_argumentPreprocessorContextDecoratorCallback (decorator);

			return this;
		}

		public ISpinvokeBindingWhenInNamedWithOrOnSyntax<T> WithReturnPostprocessorContextDecorator(
			Func<ReturnPostprocessorContext, ReturnPostprocessorContext> decorator)
		{
			_returnPostprocessorContextDecoratorCallback (decorator);

			return this;
		}
	}
}