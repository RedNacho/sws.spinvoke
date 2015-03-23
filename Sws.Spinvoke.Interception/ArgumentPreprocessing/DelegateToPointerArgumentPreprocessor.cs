using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DelegateToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		public DelegateToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode)
		{
			_pointerManagementMode = pointerManagementMode;
		}

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

