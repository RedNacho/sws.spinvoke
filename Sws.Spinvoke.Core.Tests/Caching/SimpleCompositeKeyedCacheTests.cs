using NUnit.Framework;
using System;

using Sws.Spinvoke.Core.Caching;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class SimpleCompositeKeyedCacheTests
	{
		[Test ()]
		public void FactoryInvokedForNewKey()
		{
			var cache = new SimpleCompositeKeyedCache<int> ();

			int factoryInvocationCount = 0;

			Func<int> factory = () => { factoryInvocationCount++; return 4; };

			var existingCacheKey = new CacheKey.Builder().AddComponent("Existing").Build();

			cache.GetOrAdd (existingCacheKey, () => 0);

			var cacheKey = new CacheKey.Builder ().Build ();

			var value = cache.GetOrAdd (cacheKey, factory);

			Assert.AreEqual (4, value);
			Assert.AreEqual (1, factoryInvocationCount);
		}

		[Test ()]
		public void ExistingValueReturnedForExistingKey()
		{
			var cache = new SimpleCompositeKeyedCache<int> ();

			int factoryInvocationCount = 0;

			Func<int> factory = () => 3;

			var cacheKey = new CacheKey.Builder ().Build ();

			cache.GetOrAdd (cacheKey, factory);

			factory = () => {
				factoryInvocationCount++;
				return 4;
			};

			var value = cache.GetOrAdd (cacheKey, factory);

			Assert.AreEqual (3, value);
			Assert.AreEqual (0, factoryInvocationCount);
		}
	}
}

