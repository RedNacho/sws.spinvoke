using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsStringPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsStringPointerAttribute(PointerManagementMode pointerManagementMode = PointerManagementMode.DestroyAfterCall)
			: base(new StringToPointerArgumentPreprocessor(pointerManagementMode, InterceptionAllocatedMemoryManager.PointerMemoryManager), typeof(IntPtr))
		{
		}
	}
}

