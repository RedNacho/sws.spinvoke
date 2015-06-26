using System;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class ArgumentPreprocessorContext
	{
		private readonly IInvocation _invocation;
		private readonly NativeDelegateMapping _nativeDelegateMapping;
		private readonly int _argumentIndex;

		internal ArgumentPreprocessorContext (IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping, int argumentIndex)
		{
			_invocation = invocation;
			_nativeDelegateMapping = nativeDelegateMapping;
			_argumentIndex = argumentIndex;
		}

		public IInvocation Invocation { get { return _invocation; } }
		public NativeDelegateMapping NativeDelegateMapping { get { return _nativeDelegateMapping; } }
		public int ArgumentIndex { get { return _argumentIndex; } }
	}
}

