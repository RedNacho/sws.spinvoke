using System;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public interface IContextualArgumentPreprocessor : IArgumentPreprocessor
	{
		void SetContext(ArgumentPreprocessorContext context);
	}
}

