using System;

using Ninject.Activation;

using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject.Providers
{
	public class NonNativeFallbackContext
	{
		private readonly NativeDelegateInterceptorContext _nativeDelegateInterceptorContext;

		private readonly IContext _ninjectContext;

		public NonNativeFallbackContext(NativeDelegateInterceptorContext nativeDelegateInterceptorContext,
			IContext ninjectContext)
		{
			if (nativeDelegateInterceptorContext == null)
				throw new ArgumentNullException ("nativeDelegateInterceptorContext");

			if (ninjectContext == null)
				throw new ArgumentNullException ("ninjectContext");

			_nativeDelegateInterceptorContext = nativeDelegateInterceptorContext;
			_ninjectContext = ninjectContext;
		}

		public NativeDelegateInterceptorContext NativeDelegateInterceptorContext { get { return _nativeDelegateInterceptorContext; } }

		public IContext NinjectContext { get { return _ninjectContext; } }
	}
}

