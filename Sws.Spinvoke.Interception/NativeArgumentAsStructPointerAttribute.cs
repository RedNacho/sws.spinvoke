using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsStructPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsStructPointerAttribute (bool releasePointerOnReturn = true)
			: base(new StructToPointerArgumentPreprocessor(releasePointerOnReturn), typeof(IntPtr))
		{
		}
	}
}

