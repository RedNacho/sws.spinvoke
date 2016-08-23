using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DelegateToPointerArgumentPreprocessor : IContextualArgumentPreprocessor
	{
		private readonly IContextualArgumentPreprocessor _delegateToInteropCompatibleDelegateArgumentPreprocessor;

		/// <summary>
		/// Constructor provided for backwards compatibility only. Please inject a delegateToInteropCompatibleDelegateArgumentPreprocessor.
		/// The standard implementation is DelegateToInteropCompatibleDelegateArgumentPreprocessor.
		/// </summary>
		public DelegateToPointerArgumentPreprocessor() : this(new DelegateToInteropCompatibleDelegateArgumentPreprocessor(null)) {
		}

		public DelegateToPointerArgumentPreprocessor(IContextualArgumentPreprocessor delegateToInteropCompatibleDelegateArgumentPreprocessor) {
			if (delegateToInteropCompatibleDelegateArgumentPreprocessor == null) {
				throw new ArgumentNullException ("delegateToInteropCompatibleDelegateArgumentPreprocessor");
			}

			_delegateToInteropCompatibleDelegateArgumentPreprocessor = delegateToInteropCompatibleDelegateArgumentPreprocessor;
		}

		public void SetContext (ArgumentPreprocessorContext context)
		{
			_delegateToInteropCompatibleDelegateArgumentPreprocessor.SetContext (context);
		}

		public bool CanProcess (object input)
		{
			return input is Delegate;
		}

		public object Process (object input)
		{
			var del = MapToInteropCompatibleDelegate ((Delegate) input);

			return Marshal.GetFunctionPointerForDelegate (del);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
		}

		protected virtual Delegate MapToInteropCompatibleDelegate (Delegate del) {
			if (_delegateToInteropCompatibleDelegateArgumentPreprocessor.CanProcess (del)) {
				return (Delegate) _delegateToInteropCompatibleDelegateArgumentPreprocessor.Process (del);
			}

			return del;
		}
	}
}