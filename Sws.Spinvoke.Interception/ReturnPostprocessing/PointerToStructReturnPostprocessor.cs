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
			var genericType = typeof(PtrToStructureTyped<>);
			var specificType = genericType.MakeGenericType (requiredReturnType);
			var specificInstance = Activator.CreateInstance (specificType) as PtrToStructureBase;
			var result = specificInstance.Invoke (ptr);

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

		private abstract class PtrToStructureBase
		{
			public abstract object Invoke(IntPtr ptr);
		}

		private class PtrToStructureTyped<T> : PtrToStructureBase
		{
			public override object Invoke (IntPtr ptr)
			{
				return Marshal.PtrToStructure (ptr, typeof(T));
			}
		}
	}
}

