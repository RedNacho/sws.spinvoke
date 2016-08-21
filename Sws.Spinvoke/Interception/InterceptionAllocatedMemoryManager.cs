using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Interception.MemoryManagement;

// TODO: PoC. Refactor. Check test cases.
namespace Sws.Spinvoke.Interception
{
	public static class InterceptionAllocatedMemoryManager
	{
		public static readonly PointerMemoryManager PointerMemoryManager = new FreeHGlobalPointerMemoryManager ();

		private class FreeHGlobalPointerMemoryManager : PointerMemoryManager
		{
			protected override void DefaultFreeAction (IntPtr ptr)
			{
				Marshal.FreeHGlobal (ptr);
			}
		}

		public static void BeginNamedBlock(string name)
		{
			PointerMemoryManager.BeginNamedBlock (name);
		}

		public static void EndNamedBlock()
		{
			PointerMemoryManager.EndNamedBlock ();
		}

		public static void GarbageCollectAll()
		{
			PointerMemoryManager.GarbageCollectAll ();
		}

		public static void GarbageCollectNamed(string blockName)
		{
			PointerMemoryManager.GarbageCollectNamed (blockName);
		}

		public static void GarbageCollectCurrentBlock()
		{		
			PointerMemoryManager.GarbageCollectCurrentBlock ();
		}

		public static bool HasUnnamedGarbageCollectibleMemory()
		{
			return PointerMemoryManager.HasUnnamedGarbageCollectibleMemory ();
		}

		public static IEnumerable<string> GetNamedBlocksWithGarbageCollectibleMemory()
		{
			return PointerMemoryManager.GetNamedBlocksWithGarbageCollectibleMemory ();
		}

		public static bool HasGarbageCollectibleMemory()
		{
			return PointerMemoryManager.HasGarbageCollectibleMemory ();
		}

		public static void RegisterForGarbageCollection(IntPtr ptr, Action<IntPtr> freeAction = null)
		{
			PointerMemoryManager.RegisterForGarbageCollection (ptr, freeAction);
		}

		public static void ReportPointerCallCompleted(IntPtr ptr, PointerManagementMode pointerManagementMode, Action<IntPtr> freeAction = null)
		{
			PointerMemoryManager.ReportPointerCallCompleted (ptr, pointerManagementMode, freeAction);
		}
	}
}