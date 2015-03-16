using System;

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

