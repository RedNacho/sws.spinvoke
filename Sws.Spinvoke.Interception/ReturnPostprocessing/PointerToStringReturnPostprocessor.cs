using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class PointerToStringReturnPostprocessor : IReturnPostprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		public PointerToStringReturnPostprocessor(PointerManagementMode pointerManagementMode)
		{
			_pointerManagementMode = pointerManagementMode;
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

			var result = PtrToString (ptr);

			InterceptionAllocatedMemoryManager.ReportPointerCallCompleted (ptr, _pointerManagementMode);

			return result;
		}

		protected virtual string PtrToString(IntPtr ptr)
		{
			return Marshal.PtrToStringAuto (ptr);
		}
	}
}

