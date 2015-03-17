using System;

using Sws.Spinvoke.DynamicProxy.ReturnPostprocessing;

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

