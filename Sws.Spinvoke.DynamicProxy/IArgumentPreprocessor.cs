using System;

namespace Sws.Spinvoke.DynamicProxy
{
	public interface IArgumentPreprocessor
	{
		bool CanProcess(object input);
		object Process(object input);
		void Dispose(object processedInput);
	}
}

