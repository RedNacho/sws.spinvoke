using System;
using System.Collections.Concurrent;

namespace Sws.Spinvoke.Core.Caching
{
	public class SimpleCompositeKeyedCache<TItem> : ICompositeKeyedCache<TItem>
	{
		private readonly ConcurrentDictionary<CacheKey, TItem> _cache = new ConcurrentDictionary<CacheKey, TItem>();

		public TItem GetOrAdd (CacheKey cacheKey, Func<TItem> factory)
		{
			return _cache.GetOrAdd (cacheKey, key => factory());
		}
	}
}

