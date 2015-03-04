using System;

using Sws.Nindapter.Extensions;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.Delegates;
using Sws.Spinvoke.Core.Resolver;
using Sws.Spinvoke.Core.Caching;
using Sws.Spinvoke.Core.Native;

using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Parameters;

// PoC - unit test, refactor.  Should it actually use Nindapter?
namespace Sws.Spinvoke.Ninject
{
	public class SpinvokeModule : NinjectModule
	{
		private readonly Func<IContext, object> _resolverScope;

		private readonly string _dynamicAssemblyName;

		public SpinvokeModule(Func<IContext, object> resolverScope, string dynamicAssemblyName)
		{
			if (resolverScope == null)
				throw new ArgumentNullException("resolverScope");

			if (dynamicAssemblyName == null)
				throw new ArgumentNullException("dynamicAssemblyName");

			_resolverScope = resolverScope;
			_dynamicAssemblyName = dynamicAssemblyName;
		}

		public override void Load ()
		{
			Bind<INativeDelegateResolver>().To<DefaultNativeDelegateResolver>().InScope(_resolverScope);

			Bind<CachedDelegateTypeProvider> ().ToSelf ();

			Bind<IDelegateTypeProvider> ()
				.ThroughDecorator (dtp => Kernel.Get<CachedDelegateTypeProvider>(new ConstructorArgument("innerProvider", dtp)))
				.To<DynamicAssemblyDelegateTypeProvider> ().WithConstructorArgument (_dynamicAssemblyName);

			Bind<ICompositeKeyedCache<Type>>().To<SimpleCompositeKeyedCache<Type>>().WhenInjectedInto<CachedDelegateTypeProvider>();

			Bind<INativeDelegateProvider>().To<FrameworkNativeDelegateProvider>();
		}
	}
}

