using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DelegateToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		public bool CanProcess (object input)
		{
			return input is Delegate;
		}

		public object Process (object input)
		{
			return Marshal.GetFunctionPointerForDelegate ((Delegate)input);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
		}
	}
}

