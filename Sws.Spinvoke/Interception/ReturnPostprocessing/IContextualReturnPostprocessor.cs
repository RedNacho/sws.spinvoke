using System;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public interface IContextualReturnPostprocessor : IReturnPostprocessor
	{
		void SetContext (ReturnPostprocessorContext context);
	}
}

