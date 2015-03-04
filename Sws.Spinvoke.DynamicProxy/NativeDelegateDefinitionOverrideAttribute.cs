using System;

using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.DynamicProxy
{
	public class NativeDelegateDefinitionOverrideAttribute : Attribute
	{
		public bool? MapNative { get; set; }

		public string LibraryName { get; set; }

		public string FunctionName { get; set; }

		public DelegateSignature DelegateSignature { get; set; }
	}
}

