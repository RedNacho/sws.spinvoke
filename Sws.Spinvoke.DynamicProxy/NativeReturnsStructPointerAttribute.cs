using System;

using Sws.Spinvoke.DynamicProxy.ReturnPostprocessing;

namespace Sws.Spinvoke.DynamicProxy
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class NativeReturnsStructPointerAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsStructPointerAttribute ()
			: base(new PointerToStructReturnPostprocessor(), typeof(IntPtr))
		{
		}
	}
}

