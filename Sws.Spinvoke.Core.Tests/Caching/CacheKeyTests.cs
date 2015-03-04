using NUnit.Framework;
using System;
using Sws.Spinvoke.Core.Caching;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class CacheKeyTests
	{
		[Test ()]
		public void BuilderAddComponentIncludesAllComponents ()
		{
			var builder = new CacheKey.Builder ();

			builder.AddComponent (4)
				.AddComponent ("Duck");
	
			var cacheKey = builder.Build ();

			Assert.AreEqual (2, cacheKey.Components.Length);
			Assert.AreEqual (4, cacheKey.Components [0]);
			Assert.AreEqual ("Duck", cacheKey.Components [1]);
		}

		[Test ()]
		public void BuilderAddComponentsIncludesAllComponents ()
		{
			var builder = new CacheKey.Builder ();

			builder.AddComponents (4, "Duck");

			var cacheKey = builder.Build ();

			Assert.AreEqual (2, cacheKey.Components.Length);
			Assert.AreEqual (4, cacheKey.Components [0]);
			Assert.AreEqual ("Duck", cacheKey.Components [1]);
		}

		[Test ()]
		public void SameHashCodeForSameComponents()
		{
			var cacheKey1 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();
			var cacheKey2 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();

			Assert.AreEqual (cacheKey1.GetHashCode (), cacheKey2.GetHashCode ());
		}

		[Test ()]
		public void EqualsTrueForSameComponents()
		{
			var cacheKey1 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();
			var cacheKey2 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();

			Assert.IsTrue (object.Equals (cacheKey1, cacheKey2));
		}

		[Test ()]
		public void EqualsFalseForSubsetOfComponents()
		{
			var cacheKey1 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();
			var cacheKey2 = new CacheKey.Builder ().AddComponents (1, 7).Build();

			Assert.IsFalse (object.Equals (cacheKey1, cacheKey2));
		}

		[Test ()]
		public void EqualsFalseForComponentsInDifferentOrder()
		{
			var cacheKey1 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();
			var cacheKey2 = new CacheKey.Builder ().AddComponents (1, 9, 7).Build();

			Assert.IsFalse (object.Equals (cacheKey1, cacheKey2));
		}

		public void EqualsFalseForDifferentComponents()
		{
			var cacheKey1 = new CacheKey.Builder ().AddComponents (1, 7, 9).Build();
			var cacheKey2 = new CacheKey.Builder ().AddComponents ("Duck", "Orange", "Monument").Build();

			Assert.IsFalse (object.Equals (cacheKey1, cacheKey2));
		}
	}
}

