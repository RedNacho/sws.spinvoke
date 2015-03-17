using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
{
	public class DefaultNativeArgumentDefinitionOverrideAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public DefaultNativeArgumentDefinitionOverrideAttribute (Type requiredType)
			: base(new ChangeTypeArgumentPreprocessor(requiredType))
		{
		}
	}
}

