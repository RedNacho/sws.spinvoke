using System;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public interface IContextualArgumentProcessor : IArgumentPreprocessor
	{
		void SetContext(ArgumentPreprocessorContext context);
	}
}

