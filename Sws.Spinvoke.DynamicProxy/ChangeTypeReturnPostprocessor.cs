using System;

namespace Sws.Spinvoke.DynamicProxy
{
	public class ChangeTypeReturnPostprocessor : IReturnPostprocessor
	{
		public bool CanProcess (object output, Type requiredReturnType)
		{
			return true;
		}

		public object Process (object output, Type requiredReturnType)
		{
			if (requiredReturnType.IsInstanceOfType (output)) {
				return output;
			}

			return Convert.ChangeType (output, requiredReturnType);
		}
	}
}

