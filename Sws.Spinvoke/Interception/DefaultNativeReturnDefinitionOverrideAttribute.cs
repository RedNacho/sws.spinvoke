using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	public class DefaultNativeReturnDefinitionOverrideAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public DefaultNativeReturnDefinitionOverrideAttribute (Type outputType)
			: base(new ChangeTypeReturnPostprocessor(), outputType)
		{
		}
	}
}

