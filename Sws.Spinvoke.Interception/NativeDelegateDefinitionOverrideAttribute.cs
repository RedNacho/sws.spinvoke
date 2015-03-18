using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Method)]
	public class NativeDelegateDefinitionOverrideAttribute : Attribute
	{
		private bool? _mapNative;

		private CallingConvention? _callingConvention;

		public bool MapNative
		{
			get { return _mapNative.Value; }
			set { _mapNative = value; }
		}

		public string LibraryName { get; set; }

		public string FunctionName { get; set; }

		public Type[] InputTypes { get; set; }

		public Type OutputType { get; set; }

		public CallingConvention CallingConvention
		{
			get { return _callingConvention.Value; }
			set { _callingConvention = value; }
		}

		internal bool? MapNativeNullable {
			get { return _mapNative; }
		}

		internal CallingConvention? CallingConventionNullable {
			get { return _callingConvention; }
		}
	}
}

