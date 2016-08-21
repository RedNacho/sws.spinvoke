using System;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public interface IArgumentPreprocessor
	{
		bool CanProcess(object input);
		object Process(object input);
		void ReleaseProcessedInput(object processedInput);
	}
}

