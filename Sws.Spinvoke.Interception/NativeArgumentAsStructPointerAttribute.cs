using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsStructPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsStructPointerAttribute ()
			: base(new StructToPointerArgumentPreprocessor(), typeof(IntPtr))
		{
		}
	}
}

