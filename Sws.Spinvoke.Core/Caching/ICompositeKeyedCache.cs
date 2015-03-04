using System;

namespace Sws.Spinvoke.Core.Caching
{
	public interface ICompositeKeyedCache<TItem>
	{
		TItem GetOrAdd(CacheKey cacheKey, Func<TItem> factory);
	}
}
