using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StructToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		private readonly PointerMemoryManager _pointerMemoryManager;

		[Obsolete("Please inject a PointerMemoryManager")]
		public StructToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode) : this(pointerManagementMode, InterceptionAllocatedMemoryManager.PointerMemoryManager)
		{
		}

		public StructToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager)
		{
			if (pointerMemoryManager == null) {
				throw new ArgumentNullException ("pointerMemoryManager");
			}

			_pointerManagementMode = pointerManagementMode;
			_pointerMemoryManager = pointerMemoryManager;
		}

		public bool CanProcess (object input)
		{
			return input is ValueType;
		}

		public object Process (object input)
		{
			var ptr = Marshal.AllocHGlobal (Marshal.SizeOf(input));

			Marshal.StructureToPtr (input, ptr, true);

			return ptr;
		}

		public void ReleaseProcessedInput (object processedInput)
		{
			_pointerMemoryManager.ReportPointerCallCompleted ((IntPtr)processedInput, _pointerManagementMode, Marshal.FreeHGlobal);
		}
	}
}

