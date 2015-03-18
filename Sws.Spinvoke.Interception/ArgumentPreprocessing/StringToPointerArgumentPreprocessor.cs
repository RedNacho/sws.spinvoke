using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StringToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly bool _releasePointerOnDestroy;

		public StringToPointerArgumentPreprocessor(bool releasePointerOnDestroy)
		{
			_releasePointerOnDestroy = releasePointerOnDestroy;
		}

		public bool CanProcess (object input)
		{
			return input is string;
		}

		public object Process (object input)
		{
			return Marshal.StringToHGlobalAuto ((string)input);
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

