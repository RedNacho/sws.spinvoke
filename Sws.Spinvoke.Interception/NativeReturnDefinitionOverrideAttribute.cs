using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;
using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public abstract class NativeReturnDefinitionOverrideAttribute : Attribute
	{
		private readonly IReturnPostprocessor _returnPostprocessor;

		private readonly Type _outputType;

		protected NativeReturnDefinitionOverrideAttribute(IReturnPostprocessor returnPostprocessor, Type outputType = null)
		{
			if (returnPostprocessor == null) {
				throw new ArgumentNullException("returnPostprocessor");
			}

			_returnPostprocessor = returnPostprocessor;
			_outputType = outputType;
		}

		public static PointerMemoryManager DefaultPointerMemoryManager
		{
			get { return InterceptionAllocatedMemoryManager.PointerMemoryManager; }
		}

		public IReturnPostprocessor ReturnPostprocessor
		{
			get { return _returnPostprocessor; }
		}

		public Type OutputType
		{
			get { return _outputType; }
		}
	}
}

