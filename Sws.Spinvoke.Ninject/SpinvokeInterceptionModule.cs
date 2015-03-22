using System;

using Ninject.Modules;
using Ninject.Activation;

using Sws.Spinvoke.Interception;

namespace Sws.Spinvoke.Ninject
{
	public class SpinvokeInterceptionModule : NinjectModule
	{
		private readonly Func<IContext, object> _interceptorFactoryScope;

		public SpinvokeInterceptionModule(Func<IContext, object> interceptorFactoryScope)
		{
			if (interceptorFactoryScope == null) {
				throw new ArgumentNullException ("interceptorFactoryScope");
			}

			_interceptorFactoryScope = interceptorFactoryScope;
		}

		public override void Load ()
		{
			Bind<INativeDelegateInterceptorFactory>().To<DefaultNativeDelegateInterceptorFactory> ().InScope (_interceptorFactoryScope);
		}
	}
}

