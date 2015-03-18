using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StructToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		public StructToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode)
		{
			_pointerManagementMode = pointerManagementMode;
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
			InterceptionAllocatedMemoryManager.ReportPointerCallCompleted ((IntPtr)processedInput, _pointerManagementMode);
		}
	}
}

