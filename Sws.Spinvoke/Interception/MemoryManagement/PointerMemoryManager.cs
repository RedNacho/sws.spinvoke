using System;
using System.Collections.Generic;
using System.Linq;

namespace Sws.Spinvoke.Interception.MemoryManagement
{
	public abstract class PointerMemoryManager
	{
		private readonly IDictionary<Tuple<string>, IList<Tuple<IntPtr, Action<IntPtr>>>> _freeReferences = new Dictionary<Tuple<string>, IList<Tuple<IntPtr, Action<IntPtr>>>>();

		private readonly object _syncObject = new object();

		private Tuple<string> _currentBlockName = Tuple.Create<string>(null);

		public virtual void BeginNamedBlock(string name)
		{
			lock (_syncObject)
			{
				if (name == null) {
					throw new ArgumentNullException("name");
				}

				_currentBlockName = Tuple.Create(name);
			}
		}

		public virtual void EndNamedBlock()
		{
			lock (_syncObject) {
				if (_currentBlockName.Item1 == null) {
					throw new InvalidOperationException ("Not currently in a named block.  Did you call BeginNamedBlock?");
				}

				_currentBlockName = Tuple.Create<string>(null);
			}
		}

		public virtual void GarbageCollectNamed(string blockName)
		{
			if (blockName == null) {
				throw new ArgumentNullException ("blockName");
			}

			lock (_syncObject) {
				GarbageCollectKvps (_freeReferences.Where (kvp => string.Equals (kvp.Key.Item1, blockName, StringComparison.InvariantCultureIgnoreCase)).ToArray ());
			}
		}

		public virtual void RegisterForGarbageCollection(IntPtr ptr, Action<IntPtr> freeAction = null)
		{
			freeAction = EnsureFreeAction (freeAction);

			lock (_syncObject) {
				var freeReferences = _freeReferences.ContainsKey(_currentBlockName)
					? _freeReferences[_currentBlockName]
					: (_freeReferences[_currentBlockName] = new List<Tuple<IntPtr, Action<IntPtr>>>());

				freeReferences.Add (Tuple.Create(ptr, freeAction));
			}
		}

		public virtual void ReportPointerCallCompleted(IntPtr ptr, PointerManagementMode pointerManagementMode, Action<IntPtr> freeAction = null)
		{
			freeAction = EnsureFreeAction (freeAction);

			if (pointerManagementMode == PointerManagementMode.DestroyAfterCall) {
				freeAction (ptr);
			} else if (pointerManagementMode == PointerManagementMode.DestroyOnInterceptionGarbageCollect) {
				RegisterForGarbageCollection (ptr, freeAction);
			}
		}

		public virtual bool HasGarbageCollectibleMemory()
		{
			lock (_syncObject) {
				return _freeReferences.Any ();
			}
		}

		public virtual void GarbageCollectAll ()
		{	
			lock (_syncObject) {
				GarbageCollectKvps (_freeReferences.ToArray ());
			}
		}

		public virtual void GarbageCollectCurrentBlock()
		{
			lock (_syncObject) {
				GarbageCollectKvps (_freeReferences.Where (kvp => kvp.Key == _currentBlockName).ToArray ());
			}
		}

		public virtual IEnumerable<string> GetNamedBlocksWithGarbageCollectibleMemory()
		{
			lock (_syncObject) {
				return _freeReferences.Where (kvp => kvp.Key.Item1 != null).Select (kvp => kvp.Key.Item1).ToArray();
			}
		}

		public virtual bool HasUnnamedGarbageCollectibleMemory()
		{
			lock (_syncObject) {
				return _freeReferences.Any(kvp => kvp.Key.Item1 == null);
			}
		}

		private void GarbageCollectKvps(params KeyValuePair<Tuple<string>, IList<Tuple<IntPtr, Action<IntPtr>>>>[] kvps)
		{
			var exceptionList = new List<Exception> ();

			foreach (var kvp in kvps) {
				foreach (var freeReference in kvp.Value.ToArray()) {
					try {
						freeReference.Item2(freeReference.Item1);

						kvp.Value.Remove(freeReference);

						if (kvp.Value.Count == 0) {
							_freeReferences.Remove(kvp.Key);
						}
					}
					catch (Exception ex) {
						exceptionList.Add (ex);
					}
				}
			}

			if (exceptionList.Count == 0) {
				return;
			} else if (exceptionList.Count == 1) {
				throw exceptionList.Single ();
			} else {
				throw new AggregateException (exceptionList);
			}
		}

		private Action<IntPtr> EnsureFreeAction(Action<IntPtr> freeAction)
		{
			return freeAction ?? DefaultFreeAction;
		}

		protected abstract void DefaultFreeAction (IntPtr ptr);
	}
}

