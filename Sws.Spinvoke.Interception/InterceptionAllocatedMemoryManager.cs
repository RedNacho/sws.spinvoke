using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

// TODO: PoC. Unit test, refactor.
namespace Sws.Spinvoke.Interception
{
	public static class InterceptionAllocatedMemoryManager
	{
		private static readonly IDictionary<Tuple<string>, IList<Tuple<IntPtr, Action<IntPtr>>>> FreeReferences = new Dictionary<Tuple<string>, IList<Tuple<IntPtr, Action<IntPtr>>>>();

		private static readonly object SyncObject = new object();

		private static Tuple<string> CurrentBlockName = Tuple.Create<string>(null);

		public static void BeginNamedBlock(string name)
		{
			lock (SyncObject)
			{
				if (name == null) {
					throw new ArgumentNullException("name");
				}

				CurrentBlockName = Tuple.Create(name);
			}
		}

		public static void EndNamedBlock()
		{
			lock (SyncObject) {
				if (CurrentBlockName.Item1 == null) {
					throw new InvalidOperationException ("Not currently in a named block.  Did you call BeginNamedBlock?");
				}

				CurrentBlockName = Tuple.Create<string>(null);
			}
		}

		public static void GarbageCollectAll()
		{		
			lock (SyncObject) {
				GarbageCollectKvps (FreeReferences.ToArray ());
			}
		}

		public static void GarbageCollectNamed(string blockName)
		{
			if (blockName == null) {
				throw new ArgumentNullException ("blockName");
			}

			lock (SyncObject) {
				GarbageCollectKvps (FreeReferences.Where (kvp => string.Equals (kvp.Key.Item1, blockName, StringComparison.InvariantCultureIgnoreCase)).ToArray ());
			}
		}

		public static void GarbageCollectCurrentBlock()
		{		
			lock (SyncObject) {
				GarbageCollectKvps (FreeReferences.Where (kvp => kvp.Key == CurrentBlockName).ToArray ());
			}
		}

		public static bool HasUnnamedGarbageCollectibleMemory()
		{
			lock (SyncObject) {
				return FreeReferences.Any(kvp => kvp.Key.Item1 == null);
			}
		}

		public static IEnumerable<string> GetNamedBlocksWithGarbageCollectibleMemory()
		{
			lock (SyncObject) {
				return FreeReferences.Where (kvp => kvp.Key.Item1 != null).Select (kvp => kvp.Key.Item1).ToArray();
			}
		}

		public static bool HasGarbageCollectibleMemory()
		{
			lock (SyncObject) {
				return FreeReferences.Any ();
			}
		}

		public static void RegisterForGarbageCollection(IntPtr ptr, Action<IntPtr> freeAction = null)
		{
			freeAction = EnsureFreeAction (freeAction);

			lock (SyncObject) {
				var freeReferences = FreeReferences.ContainsKey(CurrentBlockName)
					? FreeReferences[CurrentBlockName]
					: (FreeReferences[CurrentBlockName] = new List<Tuple<IntPtr, Action<IntPtr>>>());

				freeReferences.Add (Tuple.Create(ptr, freeAction));
			}
		}

		public static void ReportPointerCallCompleted(IntPtr ptr, PointerManagementMode pointerManagementMode, Action<IntPtr> freeAction = null)
		{
			freeAction = EnsureFreeAction (freeAction);

			if (pointerManagementMode == PointerManagementMode.DestroyAfterCall) {
				freeAction (ptr);
			} else if (pointerManagementMode == PointerManagementMode.DestroyOnInterceptionGarbageCollect) {
				RegisterForGarbageCollection (ptr, freeAction);
			}
		}

		private static void GarbageCollectKvps(params KeyValuePair<Tuple<string>, IList<Tuple<IntPtr, Action<IntPtr>>>>[] kvps)
		{
			var exceptionList = new List<Exception> ();

			foreach (var kvp in kvps) {
				foreach (var freeReference in kvp.Value.ToArray()) {
					try {
						freeReference.Item2(freeReference.Item1);

						kvp.Value.Remove(freeReference);

						if (kvp.Value.Count == 0) {
							FreeReferences.Remove(kvp.Key);
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

		private static Action<IntPtr> EnsureFreeAction(Action<IntPtr> freeAction)
		{
			return freeAction ?? Marshal.FreeHGlobal;
		}
	}
}

