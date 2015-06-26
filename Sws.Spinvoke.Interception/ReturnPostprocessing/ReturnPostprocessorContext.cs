using System;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class ReturnPostprocessorContext
	{
		private readonly IInvocation _invocation;
		private readonly NativeDelegateMapping _nativeDelegateMapping;
		private readonly object[] _processedArguments;
		private readonly DelegateSignature _delegateSignature;
		private readonly Delegate _delegateInstance;

		internal ReturnPostprocessorContext (IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping,
			object[] processedArguments,
			DelegateSignature delegateSignature,
			Delegate delegateInstance)
		{
			_invocation = invocation;
			_nativeDelegateMapping = nativeDelegateMapping;
			_processedArguments = processedArguments;
			_delegateSignature = delegateSignature;
			_delegateInstance = delegateInstance;
		}

		public IInvocation Invocation {
			get { return _invocation; }
		}

		public NativeDelegateMapping NativeDelegateMapping {
			get { return _nativeDelegateMapping; }
		}

		public object[] ProcessedArguments {
			get { return _processedArguments; }
		}

		public DelegateSignature DelegateSignature {
			get { return _delegateSignature; }
		}

		public Delegate DelegateInstance {
			get { return _delegateInstance; }
		}
	}
}

