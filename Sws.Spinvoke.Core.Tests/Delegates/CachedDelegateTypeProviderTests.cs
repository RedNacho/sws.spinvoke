using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;

using Moq;
using Sws.Spinvoke.Core.Delegates;
using Sws.Spinvoke.Core.Caching;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class CachedDelegateTypeProviderTests
	{
		private delegate object TestDelegate(int i, string s);

		[Test ()]
		public void CallsCacheWithCorrectCacheKey()
		{
			var inputTypes = new Type[] { typeof(int), typeof(string) };
			var outputType = typeof(object);
			var callingConvention = CallingConvention.Cdecl;

			var providerMock = new Mock<IDelegateTypeProvider> ();

			var cacheMock = new Mock<ICompositeKeyedCache<Type>> ();

			cacheMock.Setup (cache => cache.GetOrAdd (It.IsAny<CacheKey> (), It.IsAny<Func<Type>> ()))
				.Returns ((CacheKey cacheKey, Func<Type> factory) => typeof(TestDelegate));

			var subject = new CachedDelegateTypeProvider (providerMock.Object, cacheMock.Object);

			var delegateType = subject.GetDelegateType (new DelegateSignature(inputTypes, outputType, callingConvention));

			Assert.AreEqual (typeof(TestDelegate), delegateType);

			cacheMock.Verify (cache => cache.GetOrAdd (It.Is<CacheKey> (cacheKey => cacheKey.Components.SequenceEqual (
				new object[] { callingConvention, outputType, inputTypes [0], inputTypes [1] })), It.IsAny<Func<Type>> ()), Times.Once);

			cacheMock.Verify (cache => cache.GetOrAdd (It.IsAny<CacheKey> (), It.IsAny<Func<Type>> ()), Times.Once);
		}

		[Test ()]
		public void CallsCacheWithFactoryMethodInvokingProvider()
		{
			var inputTypes = new Type[] { typeof(int), typeof(string) };
			var outputType = typeof(object);
			var callingConvention = CallingConvention.Cdecl;

			var delegateSignature = new DelegateSignature (inputTypes, outputType, callingConvention);

			var providerMock = new Mock<IDelegateTypeProvider> ();

			providerMock.Setup (provider => provider.GetDelegateType (It.IsAny<DelegateSignature> ()))
				.Returns (typeof(TestDelegate));

			var cacheMock = new Mock<ICompositeKeyedCache<Type>> ();

			cacheMock.Setup (cache => cache.GetOrAdd (It.IsAny<CacheKey> (), It.IsAny<Func<Type>> ()))
				.Returns ((CacheKey cacheKey, Func<Type> factory) => factory());

			var subject = new CachedDelegateTypeProvider (providerMock.Object, cacheMock.Object);

			var delegateType = subject.GetDelegateType (delegateSignature);

			Assert.AreEqual (typeof(TestDelegate), delegateType);

			providerMock.Verify (provider => provider.GetDelegateType (delegateSignature), Times.Once);

			providerMock.Verify (provider => provider.GetDelegateType (It.IsAny<DelegateSignature> ()), Times.Once);
		}
	}
}

