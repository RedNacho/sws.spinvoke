using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsStringPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsStringPointerAttribute(bool releasePointerOnReturn = true)
			: base(new StringToPointerArgumentPreprocessor(releasePointerOnReturn), typeof(IntPtr))
		{
		}
	}
}

