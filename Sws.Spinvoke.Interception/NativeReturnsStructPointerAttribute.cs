using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class NativeReturnsStructPointerAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsStructPointerAttribute (PointerManagementMode pointerManagementMode = PointerManagementMode.DoNotDestroy)
			: base(new PointerToStructReturnPostprocessor(pointerManagementMode, DefaultPointerMemoryManager), typeof(IntPtr))
		{
		}
	}
}

