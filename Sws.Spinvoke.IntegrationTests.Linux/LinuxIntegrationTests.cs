using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Linux;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.Interception.DynamicProxy;
using Sws.Spinvoke.Ninject;
using Sws.Spinvoke.Ninject.Extensions;

using CastleProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

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

			var proxyGenerator = new CastleProxyGenerator ();

			using (var nativeDelegateResolver = kernel.Get<INativeDelegateResolver>())
			{
				var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget<IDynamicProxyTest>(new SpinvokeInterceptor(new NativeDelegateInterceptor(
					"libSws.Spinvoke.IntegrationTests.so",
					CallingConvention.Winapi,
					nativeDelegateResolver)));
			
				var result = proxy.Add(2, 3);

				Assert.AreEqual(5, result);
			}
		}

		[Test ()]
		public void NativeCodeInvokedThroughNinjectToNativeExtensionMethod()
		{
			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyTest>().ToNative("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyTest>();

			var result = proxy.Add(2, 3);

			Assert.AreEqual(5, result);
		}

		[Test ()]
		public void NativeCodeInvokedWithStructPointerConversion()
		{
			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyPointerTest>().ToNative("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyPointerTest>();

			var result = proxy.Add(2, 3);

			Assert.AreEqual(5, result);
		}

		[Test ()]
		public void NativeCodeInvokedWithStringPointerConversion()
		{
			const string TestString = "I am a test.";

			var expected = new string(TestString.Reverse ().ToArray ());

			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyStringTest>().ToNative("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyStringTest>();

			var actual = proxy.ReverseString (TestString);

			Assert.AreEqual (expected, actual);
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

	public interface IDynamicProxyStringTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsStringPointer()]
		string ReverseString([NativeArgumentAsStringPointer()] string input);
	}
}

