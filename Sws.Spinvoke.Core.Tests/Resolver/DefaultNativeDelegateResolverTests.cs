using NUnit.Framework;
using System;

using System.Runtime.InteropServices;

using Moq;

using Sws.Spinvoke.Core.Resolver;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DefaultNativeDelegateResolverTests
	{
		private delegate void TestDelegate();

		[Test ()]
		public void LoadsLibraryBasedOnResolutionRequest ()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary (It.IsAny<string> ()))
				.Returns (safeLibraryHandle);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (It.IsAny<Type> (), It.IsAny<IntPtr> ()))
				.Returns (new TestDelegate(() => { }));

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", new DelegateSignature(new Type[] { }, typeof(void), CallingConvention.Cdecl)));

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.UnloadLibrary (It.IsAny<SafeLibraryHandle>()), Times.Never);

			resolver.Dispose ();
		}

		[Test ()]
		public void RetainsLibraryThroughMultipleRequests ()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary (It.IsAny<string> ()))
				.Returns (safeLibraryHandle);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (It.IsAny<Type> (), It.IsAny<IntPtr> ()))
				.Returns (new TestDelegate(() => { }));

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", new DelegateSignature(new Type[] { }, typeof(void), CallingConvention.Cdecl)));
			resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function2", new DelegateSignature(new Type[] { }, typeof(void), CallingConvention.Cdecl)));

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.UnloadLibrary (It.IsAny<SafeLibraryHandle>()), Times.Never);

			resolver.Dispose ();
		}

		[Test ()]
		public void ReleasesLibraryOnDispose ()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary ("Test.so"))
				.Returns (safeLibraryHandle);
		
			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (It.IsAny<Type> (), It.IsAny<IntPtr> ()))
				.Returns (new TestDelegate(() => { }));

			using (var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object)) {

				resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", new DelegateSignature(new Type[] { }, typeof(void), CallingConvention.Cdecl)));
				resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function2", new DelegateSignature(new Type[] { }, typeof(void), CallingConvention.Cdecl)));

			}

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.UnloadLibrary (It.IsAny<SafeLibraryHandle>()), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.UnloadLibrary (safeLibraryHandle), Times.Once);

		}

		[Test ()]
		public void ReleasesLibraryWhenFunctionsReleased ()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary ("Test.so"))
				.Returns (safeLibraryHandle);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			int counter = 0; // Forcing it not to re-use the delegate instance...

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (It.IsAny<Type> (), It.IsAny<IntPtr> ()))
				.Returns (() => {
					var value = counter++;

					return new TestDelegate(() => {
						var capturedValue = value;
						capturedValue *= 1;
					});
				}
				);

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			var del1 = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", new DelegateSignature(new Type[] { }, typeof(void), CallingConvention.Cdecl)));
			var del2 = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function2", new DelegateSignature(new Type[] { typeof(int) }, typeof(void), CallingConvention.Cdecl)));

			resolver.Release (del1);
			resolver.Release (del2);

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.UnloadLibrary (It.IsAny<SafeLibraryHandle>()), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.UnloadLibrary (safeLibraryHandle), Times.Once);

			resolver.Dispose ();
		}

		[Test ()]
		public void ReturnsDelegateBasedOnNativeDelegateDefinition()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var funcPtr = new IntPtr (67890);

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary ("Test.so"))
				.Returns (safeLibraryHandle);

			nativeLibraryLoaderMock.Setup (nll => nll.GetFunctionPointer (safeLibraryHandle, "Function1"))
				.Returns (funcPtr);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			var delegateSignature = new DelegateSignature (new Type[] { typeof(int) }, typeof(void), CallingConvention.Cdecl);

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (delegateSignature))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr))
				.Returns (new TestDelegate(() => { }));

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			var function1 = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", delegateSignature));

			Assert.IsNotNull (function1);

			Assert.IsInstanceOf<TestDelegate> (function1);

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);

			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (safeLibraryHandle, "Function1"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (It.IsAny<SafeLibraryHandle>(), It.IsAny<string>()), Times.Once);

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (delegateSignature), Times.Once);
			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature>()), Times.Once);

			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr), Times.Once);
			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (It.IsAny<Type>(), It.IsAny<IntPtr>()), Times.Once);

			resolver.Dispose ();
		}

		[Test ()]
		public void ReturnsDelegateWithExplicitTypeIfSpecifiedInNativeDelegateDefinition()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();
			var funcPtr = new IntPtr (67890);

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary ("Test.so"))
				.Returns (safeLibraryHandle);

			nativeLibraryLoaderMock.Setup (nll => nll.GetFunctionPointer (safeLibraryHandle, "Function1"))
				.Returns (funcPtr);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()));

			var delegateSignature = new DelegateSignature (new Type[] { typeof(int) }, typeof(void), CallingConvention.Cdecl);

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr))
				.Returns (new TestDelegate(() => { }));

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			var function1 = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", delegateSignature, typeof(TestDelegate)));

			Assert.IsNotNull (function1);

			Assert.IsInstanceOf<TestDelegate> (function1);

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);

			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (safeLibraryHandle, "Function1"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (It.IsAny<SafeLibraryHandle>(), It.IsAny<string>()), Times.Once);

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature>()), Times.Never);

			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr), Times.Once);
			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (It.IsAny<Type>(), It.IsAny<IntPtr>()), Times.Once);

			resolver.Dispose ();
		}
		[Test ()]
		public void ReturnsSameDelegateWithoutCallingAnythingIfAlreadyCreated()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();
			var funcPtr = new IntPtr (67890);

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary ("Test.so"))
				.Returns (safeLibraryHandle);

			nativeLibraryLoaderMock.Setup (nll => nll.GetFunctionPointer (safeLibraryHandle, "Function1"))
				.Returns (funcPtr);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			var delegateSignature = new DelegateSignature (new Type[] { typeof(int) }, typeof(void), CallingConvention.Cdecl);
			var delegateSignature2 = new DelegateSignature (new Type[] { typeof(int) }, typeof(void), CallingConvention.Cdecl);

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (delegateSignature))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr))
				.Returns (new TestDelegate(() => { }));

			int counter = 0; // Forcing it not to re-use the delegate instance...

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr))
				.Returns (() => {
					var value = counter++;

					return new TestDelegate(() => {
						var capturedValue = value;
						capturedValue *= 1;
					});
				}
				);

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			var function1 = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", delegateSignature));

			var function1Again = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", delegateSignature2));

			Assert.AreEqual (function1, function1Again);
			Assert.IsNotNull (function1Again);

			Assert.IsInstanceOf<TestDelegate> (function1);

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);

			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (safeLibraryHandle, "Function1"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (It.IsAny<SafeLibraryHandle>(), It.IsAny<string>()), Times.Once);

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (delegateSignature), Times.Once);
			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature>()), Times.Once);

			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr), Times.Once);
			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (It.IsAny<Type>(), It.IsAny<IntPtr>()), Times.Once);

			resolver.Dispose ();
		}

		[Test ()]
		public void ReturnsDifferentDelegateIfSignatureDifferent()
		{
			var nativeLibraryLoaderMock = new Mock<INativeLibraryLoader>();

			var safeLibraryHandle = Mock.Of<SafeLibraryHandle> ();
			var funcPtr = new IntPtr (67890);

			nativeLibraryLoaderMock.Setup (nll => nll.LoadLibrary ("Test.so"))
				.Returns (safeLibraryHandle);

			nativeLibraryLoaderMock.Setup (nll => nll.GetFunctionPointer (safeLibraryHandle, "Function1"))
				.Returns (funcPtr);

			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider> ();

			var delegateSignature = new DelegateSignature (new Type[] { typeof(int) }, typeof(void), CallingConvention.Cdecl);
			var delegateSignature2 = new DelegateSignature (new Type[] { }, typeof(void), CallingConvention.Cdecl);

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (delegateSignature))
				.Returns (typeof(TestDelegate));

			delegateTypeProviderMock.Setup (dtp => dtp.GetDelegateType (delegateSignature2))
				.Returns (typeof(TestDelegate));

			var nativeDelegateProviderMock = new Mock<INativeDelegateProvider> ();

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr))
				.Returns (new TestDelegate(() => { }));

			int counter = 0; // Forcing it not to re-use the delegate instance...

			nativeDelegateProviderMock.Setup (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr))
				.Returns (() => {
					var value = counter++;

					return new TestDelegate(() => {
						var capturedValue = value;
						capturedValue *= 1;
					});
				}
				);

			var resolver = new DefaultNativeDelegateResolver (nativeLibraryLoaderMock.Object, delegateTypeProviderMock.Object, nativeDelegateProviderMock.Object);

			var function1 = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", delegateSignature));

			var function1Again = resolver.Resolve(new NativeDelegateDefinition("Test.so", "Function1", delegateSignature2));

			Assert.AreNotEqual (function1, function1Again);
			Assert.IsNotNull (function1Again);

			Assert.IsInstanceOf<TestDelegate> (function1);

			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary ("Test.so"), Times.Once);
			nativeLibraryLoaderMock.Verify (l => l.LoadLibrary (It.IsAny<string>()), Times.Once);

			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (safeLibraryHandle, "Function1"), Times.Exactly(2));
			nativeLibraryLoaderMock.Verify (l => l.GetFunctionPointer (It.IsAny<SafeLibraryHandle>(), It.IsAny<string>()), Times.Exactly(2));

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (delegateSignature), Times.Once);
			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (delegateSignature2), Times.Once);
			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature>()), Times.Exactly(2));

			nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (typeof(TestDelegate), funcPtr), Times.Exactly(2));
				nativeDelegateProviderMock.Verify (ndp => ndp.GetDelegate (It.IsAny<Type>(), It.IsAny<IntPtr>()), Times.Exactly(2));

			resolver.Dispose ();
		}

	}
}

