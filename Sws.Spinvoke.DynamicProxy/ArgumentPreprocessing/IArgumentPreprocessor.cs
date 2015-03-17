using System;

namespace Sws.Spinvoke.DynamicProxy.ArgumentPreprocessing
{
	public interface IArgumentPreprocessor
	{
		bool CanProcess(object input);
		object Process(object input);
		void DestroyProcessedInput(object processedInput);
	}
}

