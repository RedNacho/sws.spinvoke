using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StructToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly bool _releasePointerOnDestroy;

		public StructToPointerArgumentPreprocessor(bool releasePointerOnDestroy)
		{
			_releasePointerOnDestroy = releasePointerOnDestroy;
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

		public void DestroyProcessedInput (object processedInput)
		{
			if (_releasePointerOnDestroy) {
				Marshal.FreeHGlobal ((IntPtr)processedInput);
			} else {
				InterceptionAllocatedMemoryManager.ReportGarbageCollectible ((IntPtr)processedInput);
			}
		}
	}
}

