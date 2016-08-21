using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	public class NativeArgumentAsFunctionPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsFunctionPointerAttribute ()
			: base(new DelegateToPointerArgumentPreprocessor(), typeof(Delegate))
		{
		}
	}
}

