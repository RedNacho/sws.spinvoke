using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class NativeReturnsStructPointerAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsStructPointerAttribute (bool releasePointerOnReturn = true)
			: base(new PointerToStructReturnPostprocessor(releasePointerOnReturn), typeof(IntPtr))
		{
		}
	}
}

