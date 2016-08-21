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
			Delegate mappedInput = MapToInteropCompatibleDelegate ((Delegate)input);

			return Marshal.GetFunctionPointerForDelegate (mappedInput);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
		}

		protected virtual Delegate MapToInteropCompatibleDelegate (Delegate del)
		{
			return del;
		}
	}
}

