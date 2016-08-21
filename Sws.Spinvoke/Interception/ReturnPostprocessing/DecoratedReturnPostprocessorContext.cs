using System;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class DecoratedReturnPostprocessorContext<TDecoration> : ReturnPostprocessorContext, IDecorated<TDecoration> {
		private readonly TDecoration _decoration;

		public DecoratedReturnPostprocessorContext(IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping,
			object[] processedArguments,
			INativeDelegateResolver nativeDelegateResolver,
			DelegateSignature delegateSignature,
			Delegate delegateInstance,
			TDecoration decoration
		) : base(invocation, nativeDelegateMapping, processedArguments, nativeDelegateResolver,
			delegateSignature, delegateInstance) {
			this._decoration = decoration;
		}

		public TDecoration Decoration {
			get { return _decoration; }
		}
	}
}

