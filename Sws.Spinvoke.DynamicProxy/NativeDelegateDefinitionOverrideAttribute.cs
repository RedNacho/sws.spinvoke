using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.DynamicProxy
{
	[AttributeUsage(AttributeTargets.Method)]
	public class NativeDelegateDefinitionOverrideAttribute : Attribute
	{
		public bool? MapNative { get; set; }

		public string LibraryName { get; set; }

		public string FunctionName { get; set; }

		public Type[] InputTypes { get; set; }

		public Type OutputType { get; set; }

		public CallingConvention? CallingConvention { get; set; }
	}
}

