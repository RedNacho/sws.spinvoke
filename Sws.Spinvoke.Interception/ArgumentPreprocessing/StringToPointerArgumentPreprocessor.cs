using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class StringToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
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
			Marshal.FreeHGlobal ((IntPtr)processedInput);
		}
	}
}

