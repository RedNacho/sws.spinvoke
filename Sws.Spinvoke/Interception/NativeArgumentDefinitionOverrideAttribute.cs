using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public abstract class NativeArgumentDefinitionOverrideAttribute : Attribute
	{
		private readonly IArgumentPreprocessor _argumentPreprocessor;

		private readonly Type _inputType;

		protected NativeArgumentDefinitionOverrideAttribute (IArgumentPreprocessor argumentPreprocessor, Type inputType = null)
		{
			if (argumentPreprocessor == null) {
				throw new ArgumentNullException("argumentPreprocessor");
			}

			_argumentPreprocessor = argumentPreprocessor;
			_inputType = inputType;
		}

		public static PointerMemoryManager DefaultPointerMemoryManager
		{
			get { return InterceptionAllocatedMemoryManager.PointerMemoryManager; }
		}

		public IArgumentPreprocessor ArgumentPreprocessor
		{
			get { return _argumentPreprocessor; }
		}

		public Type InputType
		{
			get { return _inputType; }
		}
	}
}

