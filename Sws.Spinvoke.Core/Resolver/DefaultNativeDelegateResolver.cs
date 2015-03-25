using System;
using System.Collections.Generic;
using System.Linq;

using Sws.Spinvoke.Core.Caching;

// PoC - refactor.
namespace Sws.Spinvoke.Core.Resolver
{
	public class DefaultNativeDelegateResolver : INativeDelegateResolver
	{
		private readonly object _syncObject = new object();

		private readonly IDictionary<string, SafeLibraryHandle> _loadedLibraries = new Dictionary<string, SafeLibraryHandle>(StringComparer.InvariantCultureIgnoreCase);

		private readonly IDictionary<CacheKey, Delegate> _delegateCache = new Dictionary<CacheKey, Delegate>();

		private readonly IDictionary<Delegate, IList<NativeDelegateDefinition>> _definitionLookup = new Dictionary<Delegate, IList<NativeDelegateDefinition>>();

		private readonly IDictionary<SafeLibraryHandle, IList<CacheKey>> _libraryCacheKeys = new Dictionary<SafeLibraryHandle, IList<CacheKey>>();

		private readonly INativeLibraryLoader _nativeLibraryLoader;

		private readonly INativeDelegateProvider _nativeDelegateProvider;

		private readonly IDelegateTypeProvider _delegateTypeProvider;

		public DefaultNativeDelegateResolver(INativeLibraryLoader nativeLibraryLoader, IDelegateTypeProvider delegateTypeProvider, INativeDelegateProvider nativeDelegateProvider)
		{
			if (nativeLibraryLoader == null)
				throw new ArgumentNullException ("nativeLibraryLoader");

			if (delegateTypeProvider == null)
				throw new ArgumentNullException ("delegateTypeProvider");

			if (nativeDelegateProvider == null)
				throw new ArgumentNullException ("nativeDelegateProvider");

			_nativeLibraryLoader = nativeLibraryLoader;

			_delegateTypeProvider = delegateTypeProvider;

			_nativeDelegateProvider = nativeDelegateProvider;
		}

		public Delegate Resolve (NativeDelegateDefinition nativeDelegateDefinition)
		{
			lock (_syncObject) {
				var libHandle = ResolveLibrary (nativeDelegateDefinition.FileName);

				var cacheKey = GenerateCacheKey (libHandle, nativeDelegateDefinition);

				if (_delegateCache.ContainsKey (cacheKey)) {
					return _delegateCache [cacheKey];
				}

				var functionPointer = _nativeLibraryLoader.GetFunctionPointer (libHandle, nativeDelegateDefinition.FunctionName);

				var delegateType = nativeDelegateDefinition.ExplicitDelegateType;

				if (delegateType == null) {
					delegateType = _delegateTypeProvider.GetDelegateType (nativeDelegateDefinition.DelegateSignature);
				}

				var delegateInstance = _nativeDelegateProvider.GetDelegate (delegateType, functionPointer);

				_delegateCache [cacheKey] = delegateInstance;

				var definitionLookupList = _definitionLookup.ContainsKey (delegateInstance) ? _definitionLookup [delegateInstance] : new List<NativeDelegateDefinition> ();

				definitionLookupList.Add (nativeDelegateDefinition);

				_definitionLookup [delegateInstance] = definitionLookupList;

				var libraryCacheKeyList = _libraryCacheKeys.ContainsKey (libHandle) ? _libraryCacheKeys[libHandle] : new List<CacheKey> ();

				libraryCacheKeyList.Add (cacheKey);

				_libraryCacheKeys [libHandle] = libraryCacheKeyList;

				return delegateInstance;
			}
		}

		public void Release (Delegate nativeDelegate)
		{
			lock (_syncObject) {
				if (!_definitionLookup.ContainsKey (nativeDelegate)) {
					return;
				}

				foreach (var nativeDelegateDefinition in _definitionLookup[nativeDelegate]) {
					var libHandle = ResolveLibrary (nativeDelegateDefinition.FileName);

					var cacheKey = GenerateCacheKey (libHandle, nativeDelegateDefinition);

					if (_delegateCache.ContainsKey (cacheKey)) {
						_delegateCache.Remove (cacheKey);
					}

					if (_libraryCacheKeys.ContainsKey (libHandle)) {
						var libraryCacheKeyList = _libraryCacheKeys [libHandle];

						if (libraryCacheKeyList.Contains (cacheKey)) {
							libraryCacheKeyList.Remove (cacheKey);
						}

						if (libraryCacheKeyList.Count == 0) {
							_libraryCacheKeys.Remove (libHandle);
						}
					}

					if (!_libraryCacheKeys.ContainsKey (libHandle)) {
						ReleaseLibrary (nativeDelegateDefinition.FileName, libHandle);
					}
				}

				if (_definitionLookup.ContainsKey (nativeDelegate)) {
					_definitionLookup.Remove (nativeDelegate);
				}
			}
		}

		private SafeLibraryHandle ResolveLibrary(string fileName)
		{
			if (_loadedLibraries.ContainsKey (fileName)) {
				return _loadedLibraries [fileName];
			}

			var libHandle = _nativeLibraryLoader.LoadLibrary (fileName);
			_loadedLibraries [fileName] = libHandle;
			return libHandle;
		}

		private void ReleaseLibrary(string fileName, SafeLibraryHandle libHandle)
		{
			if (_loadedLibraries.ContainsKey (fileName)) {
				_nativeLibraryLoader.UnloadLibrary (libHandle);
				_loadedLibraries.Remove (fileName);
			}
		}

		private CacheKey GenerateCacheKey(SafeLibraryHandle libHandle, NativeDelegateDefinition nativeDelegateDefinition)
		{
			return new CacheKey.Builder ()
				.AddComponent (libHandle.GetCacheKey())
				.AddComponent (nativeDelegateDefinition.FunctionName)
				.AddComponent (nativeDelegateDefinition.DelegateSignature.GetCacheKey ())
				.AddComponent (nativeDelegateDefinition.ExplicitDelegateType)
				.Build();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				lock (_syncObject) {
					var loadedLibraries = _loadedLibraries.ToList();

					foreach (var loadedLibrary in loadedLibraries) {
						ReleaseLibrary (loadedLibrary.Key, loadedLibrary.Value);
					}
				}
			}
		}
	}
}

