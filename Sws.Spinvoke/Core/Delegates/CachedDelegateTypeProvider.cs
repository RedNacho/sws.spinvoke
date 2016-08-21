using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core.Caching;

namespace Sws.Spinvoke.Core.Delegates
{
	public class CachedDelegateTypeProvider : IDelegateTypeProvider
	{
		private readonly IDelegateTypeProvider _innerProvider;

		private readonly ICompositeKeyedCache<Type> _cache;

		public CachedDelegateTypeProvider (IDelegateTypeProvider innerProvider, ICompositeKeyedCache<Type> cache)
		{
			if (innerProvider == null)
			{
				throw new ArgumentNullException("innerProvider");
			}

			if (cache == null)
			{
				throw new ArgumentNullException ("cache");
			}

			_innerProvider = innerProvider;
			_cache = cache;
		}

		public Type GetDelegateType (DelegateSignature delegateSignature)
		{
			return _cache.GetOrAdd(GetCacheKey(delegateSignature),
				() => _innerProvider.GetDelegateType(delegateSignature));
		}

		private CacheKey GetCacheKey(DelegateSignature delegateSignature)
		{
			return delegateSignature.GetCacheKey ();
		}
	}
}

