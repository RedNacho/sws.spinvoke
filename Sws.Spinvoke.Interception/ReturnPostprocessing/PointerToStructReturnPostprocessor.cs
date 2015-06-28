using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class PointerToStructReturnPostprocessor : IReturnPostprocessor
	{
		private readonly PointerMemoryManager _pointerMemoryManager;

		private readonly PointerManagementMode _pointerManagementMode;

		[Obsolete("Please inject a PointerMemoryManager")]
		public PointerToStructReturnPostprocessor(PointerManagementMode pointerManagementMode) : this(pointerManagementMode, InterceptionAllocatedMemoryManager.PointerMemoryManager)
		{
		}

		public PointerToStructReturnPostprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager)
		{
			if (pointerMemoryManager == null) {
				throw new ArgumentNullException ("pointerMemoryManager");
			}

			_pointerManagementMode = pointerManagementMode;
			_pointerMemoryManager = pointerMemoryManager;
		}

		public bool CanProcess (object output, Type requiredReturnType)
		{
			return output is IntPtr && requiredReturnType.IsValueType;
		}

		public object Process (object output, Type requiredReturnType)
		{
			IntPtr ptr = (IntPtr)output;

			var result = Marshal.PtrToStructure (ptr, requiredReturnType);

			_pointerMemoryManager.ReportPointerCallCompleted (ptr, _pointerManagementMode, IsFreePointerImplemented ? (Action<IntPtr>)FreePointer : null);

			return result;
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

