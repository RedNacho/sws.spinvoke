using System;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class ChangeTypeReturnPostprocessor : IReturnPostprocessor
	{
		public bool CanProcess (object output, Type requiredReturnType)
		{
			return true;
		}

		public object Process (object output, Type requiredReturnType)
		{
			if (requiredReturnType == typeof(void)) {
				return null;
			}

			if (requiredReturnType.IsInstanceOfType (output)) {
				return output;
			}

			return Convert.ChangeType (output, requiredReturnType);
		}
	}
}

