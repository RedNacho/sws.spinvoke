using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class PointerToStructReturnPostprocessor : IReturnPostprocessor
	{
		private readonly PointerManagementMode _pointerManagementMode;

		public PointerToStructReturnPostprocessor(PointerManagementMode pointerManagementMode)
		{
			_pointerManagementMode = pointerManagementMode;
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

			InterceptionAllocatedMemoryManager.ReportPointerCallCompleted (ptr, _pointerManagementMode);

			return result;
		}

		private abstract class PtrToStructureBase
		{
			public abstract object Invoke(IntPtr ptr);
		}

		private class PtrToStructureTyped<T> : PtrToStructureBase
		{
			public override object Invoke (IntPtr ptr)
			{
				return Marshal.PtrToStructure<T> (ptr);
			}
		}
	}
}

