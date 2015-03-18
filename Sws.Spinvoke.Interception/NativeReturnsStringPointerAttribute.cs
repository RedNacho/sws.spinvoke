using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class NativeReturnsStringPointerAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsStringPointerAttribute()
			: base(new PointerToStringReturnPostprocessor(), typeof(IntPtr))
		{
		}
	}
}

