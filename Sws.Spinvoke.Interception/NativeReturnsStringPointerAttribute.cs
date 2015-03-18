using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class NativeReturnsStringPointerAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsStringPointerAttribute(bool releasePointerOnReturn = true)
			: base(new PointerToStringReturnPostprocessor(releasePointerOnReturn), typeof(IntPtr))
		{
		}
	}
}

