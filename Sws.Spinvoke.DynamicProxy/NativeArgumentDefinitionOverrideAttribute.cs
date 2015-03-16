using System;

namespace Sws.Spinvoke.DynamicProxy
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentDefinitionOverrideAttribute : Attribute
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

