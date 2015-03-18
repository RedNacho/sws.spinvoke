using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StringToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		public StringToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode)
		{
			_pointerManagementMode = pointerManagementMode;
		}

		public bool CanProcess (object input)
		{
			return input is string;
		}

		public object Process (object input)
		{
			return Marshal.StringToHGlobalAuto ((string)input);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
			InterceptionAllocatedMemoryManager.ReportPointerCallCompleted ((IntPtr)processedInput, _pointerManagementMode);
		}
	}
}

