using System;

using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Interception.Facade
{
	// TODO More tests!
	public class SpinvokeInterceptionFacade
	{
		private readonly INativeDelegateInterceptorFactory _nativeDelegateInterceptorFactory;

		private SpinvokeInterceptionFacade (INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory)
		{
			if (nativeDelegateInterceptorFactory == null)
				throw new ArgumentNullException ("nativeDelegateInterceptorFactory");

			_nativeDelegateInterceptorFactory = nativeDelegateInterceptorFactory;
		}

		public INativeDelegateInterceptorFactory NativeDelegateInterceptorFactory
		{
			get { return _nativeDelegateInterceptorFactory; }
		}

		public class Builder
		{
			private Func<INativeDelegateInterceptorFactory> _nativeDelegateInterceptorFactoryFactory;

			public Builder()
			{
				_nativeDelegateInterceptorFactoryFactory = () => new DefaultNativeDelegateInterceptorFactory();
			}

			public Builder WithNativeDelegateInterceptorFactory(INativeDelegateInterceptorFactory nativeDelegateInterceptorFactory)
			{
				_nativeDelegateInterceptorFactoryFactory = () => nativeDelegateInterceptorFactory;

				return this;
			}

			public SpinvokeInterceptionFacade Build()
			{
				var nativeDelegateInterceptorFactory = new Lazy<INativeDelegateInterceptorFactory> (_nativeDelegateInterceptorFactoryFactory);

				return new SpinvokeInterceptionFacade (nativeDelegateInterceptorFactory.Value);
			}
		}
	}
}

