using NUnit.Framework;
using System;
using System.Collections.Generic;
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

using Moq;

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

		[Test ()]
		public void NativeCodeInvokedThroughExplicitDelegateTypeIfSpecified()
		{
			const int X = 2;
			const int Y = 3;

			const int Expected = 5;

			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyExplicitDelegateTypeTest>().ToNative("libSws.Spinvoke.IntegrationTests.so").WithCallingConvention(CallingConvention.FastCall);

			var proxy = kernel.Get<IDynamicProxyExplicitDelegateTypeTest>();

			var actual = proxy.Add (X, Y);

			Assert.AreEqual (Expected, actual);
		}

		[Test ()]
		public void InterceptionAllocatedMemoryManagerAllowsManualDeallocationOfGeneratedPointers()
		{
			const string TestString = "I am a test.";

			var expected = new string(TestString.Reverse ().ToArray ());

			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyManualMemoryReleaseTest>().ToNative("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyManualMemoryReleaseTest>();

			var actual = proxy.ReverseString (TestString);

			var hasGarbageCollectibleMemoryBefore = InterceptionAllocatedMemoryManager.HasGarbageCollectibleMemory ();

			InterceptionAllocatedMemoryManager.GarbageCollectAll ();

			var hasGarbageCollectibleMemoryAfter = InterceptionAllocatedMemoryManager.HasGarbageCollectibleMemory ();

			Assert.AreEqual (expected, actual);
			Assert.IsTrue (hasGarbageCollectibleMemoryBefore);
			Assert.IsFalse (hasGarbageCollectibleMemoryAfter);
		}

		[Test ()]
		[ExpectedException (typeof(NotImplementedException))]
		public void ProxyThrowsNotImplementedIfNoFallbackAndUnmapped()
		{
			const int TestInput = -5;

			var kernel = new StandardKernel();

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyUnmappedTest> ().ToNative ("libSws.Spinvoke.IntegrationTests.so");

			var proxy = kernel.Get<IDynamicProxyUnmappedTest>();

			proxy.Abs (TestInput);
		}

		[Test ()]
		[ExpectedException (typeof(InvalidOperationException))]
		public void WithNonNativeFallbackThrowsInvalidOperationExceptionIfProxyGeneratorDoesNotSupportTargets()
		{
			const int TestInput = -5;

			var kernel = new StandardKernel();

			var noTargetProxyGeneratorMock = new Mock<IProxyGenerator> ();

			var nonNativeFallbackMock = new Mock<IDynamicProxyUnmappedTest> ();

			noTargetProxyGeneratorMock.Setup (pg => pg.AllowsTarget).Returns (false);

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), noTargetProxyGeneratorMock.Object);

			kernel.Bind<IDynamicProxyUnmappedTest> ().ToNative ("libSws.Spinvoke.IntegrationTests.so")
				.WithNonNativeFallback (context => nonNativeFallbackMock.Object);
		}

		[Test ()]
		public void ProxyPassesThroughToFallbackIfUnmapped()
		{
			const int TestInput = -5;

			const int Expected = 5;

			var kernel = new StandardKernel();

			var nonNativeFallbackContexts = new List<INonNativeFallbackContext> ();

			var nonNativeFallbackMock = new Mock<IDynamicProxyUnmappedTest> ();

			nonNativeFallbackMock.Setup (nnf => nnf.Abs (It.IsAny<int> ()))
				.Returns ((int value) => Math.Abs (value));

			SpinvokeNinjectExtensionsConfiguration.Configure (new LinuxNativeLibraryLoader(), new ProxyGenerator(new CastleProxyGenerator()));

			kernel.Bind<IDynamicProxyUnmappedTest>().ToNative("libSws.Spinvoke.IntegrationTests.so")
				.WithCallingConvention(CallingConvention.FastCall)
				.WithNonNativeFallback(nnfc => {
					nonNativeFallbackContexts.Add(nnfc);
					return nonNativeFallbackMock.Object;
				});

			var proxy = kernel.Get<IDynamicProxyUnmappedTest>();

			var actual = proxy.Abs (TestInput);

			nonNativeFallbackMock.Verify (nnf => nnf.Abs (TestInput), Times.Once);
			nonNativeFallbackMock.Verify (nnf => nnf.Abs (It.IsAny<int> ()), Times.Once);

			Assert.AreEqual (1, nonNativeFallbackContexts.Count ());

			var nonNativeFallbackContext = nonNativeFallbackContexts.Single();

			Assert.AreEqual("libSws.Spinvoke.IntegrationTests.so", nonNativeFallbackContext.LibraryName);
			Assert.AreEqual(CallingConvention.FastCall, nonNativeFallbackContext.CallingConvention);
			Assert.IsNotNull(nonNativeFallbackContext.NativeDelegateResolver);
			Assert.IsNotNull(nonNativeFallbackContext.NinjectContext);

			Assert.AreEqual (Expected, actual);
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
		[return: NativeReturnsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		int Add([NativeArgumentAsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] int x, [NativeArgumentAsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] int y);
	}

	public interface IDynamicProxyStringTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		string ReverseString([NativeArgumentAsStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] string input);
	}

	public interface IDynamicProxyManualMemoryReleaseTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		string ReverseString([NativeArgumentAsStringPointer(pointerManagementMode: PointerManagementMode.DestroyOnInterceptionGarbageCollect)] string input);
	}

	public delegate int ExplicitAddDelegate(int x, int y);

	public interface IDynamicProxyExplicitDelegateTypeTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "add", ExplicitDelegateType = typeof(ExplicitAddDelegate))]
		decimal Add(int x, int y);
	}

	public interface IDynamicProxyUnmappedTest
	{
		[NativeDelegateDefinitionOverride(MapNative = false)]
		int Abs(int value);
	}
}

