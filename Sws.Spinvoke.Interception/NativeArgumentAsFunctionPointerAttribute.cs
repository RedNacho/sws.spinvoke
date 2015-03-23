using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	public class NativeArgumentAsFunctionPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsFunctionPointerAttribute (PointerManagementMode pointerManagementMode)
			: base(new DelegateToPointerArgumentPreprocessor(pointerManagementMode), typeof(Delegate))
		{
		}
	}
}

