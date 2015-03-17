using System;

using Sws.Spinvoke.DynamicProxy.ArgumentPreprocessing;

namespace Sws.Spinvoke.DynamicProxy
{
	public class DefaultNativeArgumentDefinitionOverrideAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public DefaultNativeArgumentDefinitionOverrideAttribute (Type requiredType)
			: base(new ChangeTypeArgumentPreprocessor(requiredType))
		{
		}
	}
}

