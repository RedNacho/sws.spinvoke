using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

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

