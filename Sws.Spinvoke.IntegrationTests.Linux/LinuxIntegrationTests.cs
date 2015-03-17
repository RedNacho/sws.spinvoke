using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Linux;
using Sws.Spinvoke.DynamicProxy;
using Sws.Spinvoke.DynamicProxy.ArgumentPreprocessing;
using Sws.Spinvoke.DynamicProxy.ReturnPostprocessing;
using Sws.Spinvoke.Ninject;
using Sws.Spinvoke.Ninject.Extensions;

using Castle.DynamicProxy;

using Ninject;
using Ninject.Infrastructure;

namespace Sws.Spinvoke.IntegrationTests.Linux
{
	[TestFixture ()]
	public class LinuxIntegrationTests
	{
		[Test ()]
		public void NativeCodeInvokedThroughGeneratedDelegate ()
		{
			var kernel = new StandardKernel ();

			kernel.Bind<INativeLibraryLoader>().To<LinuxNativeLibraryLoader> ();

			kernel.Load (new SpinvokeModule (StandardScopeCallbacks.Transient, "TestAssembly"));

			using (var nativeDelegateResolver = kernel.Get<INativeDelegateResolver>())
			{
				var delegateInstance = nativeDelegateResolver.Resolve(new NativeDelegateDefinition("libSws.Spinvoke.IntegrationTests.so", "add",
					new DelegateSignature(new [] { typeof(int), typeof(int) }, typeof(int), CallingConvention.Cdecl)));

				var result = delegateInstance.DynamicInvoke(new object[] { 2, 3 });

				Assert.AreEqual(5, result);
			}
		}

		[Test ()]
		public void NativeCodeInvokedThroughDynamicProxy()
		{
			var kernel = new StandardKernel ();

			kernel.Bind<INativeLibraryLoader>().To<LinuxNativeLibraryLoader> ();

			kernel.Load (new SpinvokeModule (StandardScopeCallbacks.Transient, "TestAssembly"));

			var proxyGenerator = new ProxyGenerator ();

			using (var nativeDelegateResolver = kernel.Get<INativeDelegateResolver>())
			{
				var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget<IDynamicProxyTest>(new NativeDelegateInterceptor(
					"libSws.Spinvoke.IntegrationTests.so",
					CallingConvention.Winapi,
					nativeDelegateResolver));
			
				var result = proxy.Add(2, 3);

				Assert.AreEqual(5, result);
			}
		}

		[Test ()]
		public void NativeCodeInvokedThroughNinjectToNativeExtensionMethod()
		{
			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader());

			kernel.Bind<IDynamicProxyTest>().ToNative("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyTest>();

			var result = proxy.Add(2, 3);

			Assert.AreEqual(5, result);
		}

		[Test ()]
		public void NativeCodeInvokedWithPointerConversion()
		{
			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader());

			kernel.Bind<IDynamicProxyPointerTest>().ToNative("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyPointerTest>();

			var result = proxy.Add(2, 3);

			Assert.AreEqual(5, result);
		}
	}

	public interface IDynamicProxyTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "add")]
		int Add(int x, int y);
	}

	public interface IDynamicProxyPointerTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "pointerAdd")]
		[return: NativeReturnsStructPointer()]
		int Add([NativeArgumentAsStructPointer()] int x, [NativeArgumentAsStructPointer()] int y);
	}
}

