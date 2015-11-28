using System;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.Delegates;
using Sws.Spinvoke.Core.Expressions;
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

			Bind<DynamicAssemblyDelegateTypeProvider> ().ToSelf ()
				.WithConstructorArgument("assemblyName", _dynamicAssemblyName);

			Bind<IDelegateTypeProvider> ()
				.ToMethod (ctx =>
					Kernel.Get<CachedDelegateTypeProvider>(
						new ConstructorArgument("innerProvider",
							Kernel.Get<DynamicAssemblyDelegateTypeProvider>())))
				.InSingletonScope();

			Bind<ICompositeKeyedCache<Type>>().To<SimpleCompositeKeyedCache<Type>>().WhenInjectedInto<CachedDelegateTypeProvider>();

			Bind<INativeDelegateProvider>().To<FrameworkNativeDelegateProvider>();

			Bind<IDelegateTypeToDelegateSignatureConverter> ().To<DefaultDelegateTypeToDelegateSignatureConverter> ();

			Bind<IDelegateExpressionBuilder> ().To<DefaultDelegateExpressionBuilder> ();

			Bind<INativeExpressionBuilder> ().To<DefaultNativeExpressionBuilder> ();
		}
	}
}

