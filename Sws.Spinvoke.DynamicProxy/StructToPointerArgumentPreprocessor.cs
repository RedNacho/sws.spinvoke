using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.DynamicProxy
{
	public class StructToPointerArgumentPreprocessor : IArgumentPreprocessor
	{
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

		public void Dispose (object processedInput)
		{
			Marshal.FreeHGlobal ((IntPtr)processedInput);
		}
	}
}

