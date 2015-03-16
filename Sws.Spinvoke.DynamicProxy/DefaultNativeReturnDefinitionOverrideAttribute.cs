using System;

namespace Sws.Spinvoke.DynamicProxy
{
	public class DefaultNativeReturnDefinitionOverrideAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public DefaultNativeReturnDefinitionOverrideAttribute ()
			: base(new ChangeTypeReturnPostprocessor())
		{
		}
	}
}

