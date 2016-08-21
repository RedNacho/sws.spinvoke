using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StringToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		private readonly PointerMemoryManager _pointerMemoryManager;

		[Obsolete("Please inject a PointerMemoryManager")]
		public StringToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode) : this(pointerManagementMode, InterceptionAllocatedMemoryManager.PointerMemoryManager)
		{
		}

		public StringToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager)
		{
			if (pointerMemoryManager == null) {
				throw new ArgumentNullException ("pointerMemoryManager");
			}

			_pointerManagementMode = pointerManagementMode;
			_pointerMemoryManager = pointerMemoryManager;
		}

		public bool CanProcess (object input)
		{
			return input is string;
		}

		public object Process (object input)
		{
			return StringToPointer ((string)input);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
			_pointerMemoryManager.ReportPointerCallCompleted ((IntPtr)processedInput, _pointerManagementMode, FreePointer);
		}

		protected virtual IntPtr StringToPointer(string input)
		{
			return Marshal.StringToHGlobalAuto (input);
		}

		protected virtual void FreePointer(IntPtr pointer)
		{
			Marshal.FreeHGlobal (pointer);
		}
	}
}

