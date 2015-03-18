using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class PointerToStringReturnPostprocessor : IReturnPostprocessor
	{
		private readonly bool _releasePointerOnProcess;

		public PointerToStringReturnPostprocessor(bool releasePointerOnProcess)
		{
			_releasePointerOnProcess = releasePointerOnProcess;
		}

		public bool CanProcess (object output, Type requiredReturnType)
		{
			return output is IntPtr && requiredReturnType.IsAssignableFrom(typeof(string));
		}

		public object Process (object output, Type requiredReturnType)
		{
			if (!requiredReturnType.IsAssignableFrom (typeof(string))) {
				throw new ArgumentException ("requiredReturnType must be assignable from string.");
			}

			var ptr = (IntPtr)output;

			var result = Marshal.PtrToStringAuto ((IntPtr)output);

			if (_releasePointerOnProcess) {
				Marshal.FreeHGlobal (ptr);
			}

			return result;
		}
	}
}

