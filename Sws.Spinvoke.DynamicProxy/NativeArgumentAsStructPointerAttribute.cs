using System;

using Sws.Spinvoke.DynamicProxy.ArgumentPreprocessing;

namespace Sws.Spinvoke.DynamicProxy
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

