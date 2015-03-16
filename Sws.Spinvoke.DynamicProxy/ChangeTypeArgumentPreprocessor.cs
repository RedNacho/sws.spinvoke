using System;

namespace Sws.Spinvoke.DynamicProxy
{
	public class ChangeTypeArgumentPreprocessor : IArgumentPreprocessor
	{
		private readonly Type _requiredType;

		public ChangeTypeArgumentPreprocessor(Type requiredType)
		{
			if (requiredType == null) {
				throw new ArgumentNullException ("requiredType");
			}

			_requiredType = requiredType;
		}

		public bool CanProcess (object input)
		{
			return true;
		}

		public object Process (object input)
		{
			if (_requiredType.IsInstanceOfType (input)) {
				return input;
			}

			return Convert.ChangeType (input, _requiredType);
		}

		public void DestroyProcessedInput (object processedInput)
		{
			var disposable = processedInput as IDisposable;

			if (disposable != null) {
				disposable.Dispose ();
			}
		}
	}
}

