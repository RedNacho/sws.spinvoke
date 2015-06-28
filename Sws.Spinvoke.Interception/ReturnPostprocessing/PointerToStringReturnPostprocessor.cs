using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class PointerToStringReturnPostprocessor : IReturnPostprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		private readonly PointerMemoryManager _pointerMemoryManager;

		[Obsolete("Please inject a PointerMemoryManager")]
		public PointerToStringReturnPostprocessor(PointerManagementMode pointerManagementMode) : this(pointerManagementMode, InterceptionAllocatedMemoryManager.PointerMemoryManager)
		{
		}

		public PointerToStringReturnPostprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager)
		{
			if (pointerMemoryManager == null) {
				throw new ArgumentNullException ("pointerMemoryManager");
			}

			_pointerManagementMode = pointerManagementMode;
			_pointerMemoryManager = pointerMemoryManager;
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

			_pointerMemoryManager.ReportPointerCallCompleted (ptr, _pointerManagementMode, IsFreePointerImplemented ? (Action<IntPtr>)FreePointer : null);

			return result;
		}

		protected virtual string PtrToString(IntPtr ptr)
		{
			return Marshal.PtrToStringAuto (ptr);
		}

		protected virtual bool IsFreePointerImplemented
		{
			get {
				return false;
			}
		}

		protected virtual void FreePointer(IntPtr pointer)
		{
			throw new NotImplementedException ();
		}
	}
}

