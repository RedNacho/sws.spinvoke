using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsStructPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsStructPointerAttribute (PointerManagementMode pointerManagementMode = PointerManagementMode.DestroyAfterCall)
			: base(new StructToPointerArgumentPreprocessor(pointerManagementMode, InterceptionAllocatedMemoryManager.PointerMemoryManager), typeof(IntPtr))
		{
		}
	}
}

