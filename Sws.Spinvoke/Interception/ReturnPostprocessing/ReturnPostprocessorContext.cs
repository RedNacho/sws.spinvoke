using System;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class ReturnPostprocessorContext
	{
		private readonly IInvocation _invocation;
		private readonly NativeDelegateMapping _nativeDelegateMapping;
		private readonly object[] _processedArguments;
		private readonly INativeDelegateResolver _nativeDelegateResolver;
		private readonly DelegateSignature _delegateSignature;
		private readonly Delegate _delegateInstance;

		public ReturnPostprocessorContext (IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping,
			object[] processedArguments,
			INativeDelegateResolver nativeDelegateResolver,
			DelegateSignature delegateSignature,
			Delegate delegateInstance)
		{
			_invocation = invocation;
			_nativeDelegateMapping = nativeDelegateMapping;
			_processedArguments = processedArguments;
			_nativeDelegateResolver = nativeDelegateResolver;
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

		public INativeDelegateResolver NativeDelegateResolver {
			get { return _nativeDelegateResolver; }
		}

		public DelegateSignature DelegateSignature {
			get { return _delegateSignature; }
		}

		public Delegate DelegateInstance {
			get { return _delegateInstance; }
		}

		public DecoratedReturnPostprocessorContext<TDecoration> DecorateWith<TDecoration>(TDecoration decoration) {
			return new DecoratedReturnPostprocessorContext<TDecoration> (
				Invocation,
				NativeDelegateMapping,
				ProcessedArguments,
				NativeDelegateResolver,
				DelegateSignature,
				DelegateInstance,
				decoration
			);
		}
	}
}

