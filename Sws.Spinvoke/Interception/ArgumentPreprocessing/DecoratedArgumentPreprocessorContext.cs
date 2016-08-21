using System;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DecoratedArgumentPreprocessorContext<TDecoration> : ArgumentPreprocessorContext, IDecorated<TDecoration> {
		private readonly TDecoration _decoration;

		public DecoratedArgumentPreprocessorContext(IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping,
			int argumentIndex,
			TDecoration decoration) : base(invocation, nativeDelegateMapping, argumentIndex) {
			_decoration = decoration;
		}

		public TDecoration Decoration {
			get { return _decoration; }
		}
	}
}

