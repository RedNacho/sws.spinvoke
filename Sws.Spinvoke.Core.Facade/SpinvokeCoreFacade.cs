using System;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.Caching;
using Sws.Spinvoke.Core.Delegates;
using Sws.Spinvoke.Core.Expressions;
using Sws.Spinvoke.Core.Native;
using Sws.Spinvoke.Core.Resolver;

namespace Sws.Spinvoke.Core.Facade
{
	// TODO More tests!
	public class SpinvokeCoreFacade : IDisposable
	{
		private readonly INativeDelegateResolver _nativeDelegateResolver;

		private readonly INativeExpressionBuilder _nativeExpressionBuilder;

		private SpinvokeCoreFacade (INativeDelegateResolver nativeDelegateResolver, INativeExpressionBuilder nativeExpressionBuilder)
		{
			if (nativeDelegateResolver == null) {
				throw new ArgumentNullException ("nativeDelegateResolver");
			}

			if (nativeExpressionBuilder == null) {
				throw new ArgumentNullException ("nativeExpressionBuilder");
			}

			_nativeDelegateResolver = nativeDelegateResolver;
			_nativeExpressionBuilder = nativeExpressionBuilder;
		}

		public INativeDelegateResolver NativeDelegateResolver
		{
			get { return _nativeDelegateResolver; }
		}

		public INativeExpressionBuilder NativeExpressionBuilder
		{
			get { return _nativeExpressionBuilder; }
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				this.NativeDelegateResolver.Dispose ();
			}
		}

		public class Builder
		{
			private Func<IDelegateTypeProvider> _uncachedDelegateTypeProviderFactory;

			private Func<ICompositeKeyedCache<Type>> _delegateTypeProviderCacheImplementationFactory;

			private Func<Lazy<IDelegateTypeProvider>, Lazy<ICompositeKeyedCache<Type>>, IDelegateTypeProvider> _delegateTypeProviderFactory;

			private Func<INativeDelegateProvider> _nativeDelegateProviderFactory;

			private Func<Lazy<IDelegateTypeProvider>, Lazy<INativeDelegateProvider>, INativeDelegateResolver> _nativeDelegateResolverFactory;

			private Func<IDelegateTypeToDelegateSignatureConverter> _delegateTypeToDelegateSignatureConverterFactory;

			private Func<IDelegateExpressionBuilder> _delegateExpressionBuilderFactory;

			private Func<Lazy<INativeDelegateResolver>, Lazy<IDelegateTypeToDelegateSignatureConverter>, Lazy<IDelegateExpressionBuilder>, INativeExpressionBuilder> _nativeExpressionBuilderFactory;

			public Builder(INativeLibraryLoader nativeLibraryLoader, string dynamicAssemblyName)
			{
				_uncachedDelegateTypeProviderFactory = () => new DynamicAssemblyDelegateTypeProvider(dynamicAssemblyName);

				_delegateTypeProviderCacheImplementationFactory = () => new SimpleCompositeKeyedCache<Type>();

				_delegateTypeProviderFactory = (uncachedDelegateTypeProvider, delegateTypeProviderCacheImplementation) => new CachedDelegateTypeProvider(uncachedDelegateTypeProvider.Value, delegateTypeProviderCacheImplementation.Value);

				_nativeDelegateProviderFactory = () => new FrameworkNativeDelegateProvider();

				_nativeDelegateResolverFactory = (delegateTypeProvider, nativeDelegateProvider) => new DefaultNativeDelegateResolver(nativeLibraryLoader, delegateTypeProvider.Value, nativeDelegateProvider.Value);

				_delegateTypeToDelegateSignatureConverterFactory = () => new DefaultDelegateTypeToDelegateSignatureConverter();

				_delegateExpressionBuilderFactory = () => new DefaultDelegateExpressionBuilder();

				_nativeExpressionBuilderFactory = (nativeDelegateResolver, delegateTypeToDelegateSignatureConverter, delegateExpressionBuilder)
					=> new DefaultNativeExpressionBuilder(nativeDelegateResolver.Value, delegateTypeToDelegateSignatureConverter.Value, delegateExpressionBuilder.Value);
			}

			public Builder WithUncachedDelegateTypeProvider(IDelegateTypeProvider uncachedDelegateTypeProvider)
			{
				_uncachedDelegateTypeProviderFactory = () => uncachedDelegateTypeProvider;

				return this;
			}

			public Builder WithDelegateTypeProviderCacheImplementation(ICompositeKeyedCache<Type> delegateTypeProviderCacheImplementation)
			{
				_delegateTypeProviderCacheImplementationFactory = () => delegateTypeProviderCacheImplementation;

				return this;
			}

			public Builder WithDelegateTypeProvider(IDelegateTypeProvider delegateTypeProvider)
			{
				_delegateTypeProviderFactory = (a, b) => delegateTypeProvider;

				return this;
			}

			public Builder WithNativeDelegateProvider(INativeDelegateProvider nativeDelegateProvider)
			{
				_nativeDelegateProviderFactory = () => nativeDelegateProvider;

				return this;
			}

			public Builder WithNativeDelegateResolver(INativeDelegateResolver nativeDelegateResolver)
			{
				_nativeDelegateResolverFactory = (a, b) => nativeDelegateResolver;

				return this;
			}

			public Builder WithDelegateTypeToDelegateSignatureConverter(IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter)
			{
				_delegateTypeToDelegateSignatureConverterFactory = () => delegateTypeToDelegateSignatureConverter;

				return this;
			}

			public Builder WithDelegateExpressionBuilder(IDelegateExpressionBuilder delegateExpressionBuilder)
			{
				_delegateExpressionBuilderFactory = () => delegateExpressionBuilder;

				return this;
			}

			public Builder WithNativeExpressionBuilder(INativeExpressionBuilder nativeExpressionBuilder)
			{
				_nativeExpressionBuilderFactory = (a, b, c) => nativeExpressionBuilder;

				return this;
			}

			public SpinvokeCoreFacade Build()
			{
				var uncachedDelegateTypeProvider = new Lazy<IDelegateTypeProvider> (_uncachedDelegateTypeProviderFactory);

				var delegateTypeProviderCacheImplementation = new Lazy<ICompositeKeyedCache<Type>> (_delegateTypeProviderCacheImplementationFactory);

				var delegateTypeProvider = new Lazy<IDelegateTypeProvider>(() => _delegateTypeProviderFactory(uncachedDelegateTypeProvider, delegateTypeProviderCacheImplementation));

				var nativeDelegateProvider = new Lazy<INativeDelegateProvider>(_nativeDelegateProviderFactory);

				var nativeDelegateResolver = new Lazy<INativeDelegateResolver>(() => _nativeDelegateResolverFactory(delegateTypeProvider, nativeDelegateProvider));

				var delegateTypeToDelegateSignatureConverter = new Lazy<IDelegateTypeToDelegateSignatureConverter> (_delegateTypeToDelegateSignatureConverterFactory);

				var delegateExpressionBuilder = new Lazy<IDelegateExpressionBuilder> (_delegateExpressionBuilderFactory);

				var nativeExpressionBuilder = new Lazy<INativeExpressionBuilder> (() => _nativeExpressionBuilderFactory (nativeDelegateResolver, delegateTypeToDelegateSignatureConverter, delegateExpressionBuilder));

				return new SpinvokeCoreFacade (nativeDelegateResolver.Value, nativeExpressionBuilder.Value);
			}
		}
	}
}

