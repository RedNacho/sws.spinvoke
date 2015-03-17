using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class PointerToStructReturnPostprocessor : IReturnPostprocessor
	{
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
			Marshal.FreeHGlobal (ptr);
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

