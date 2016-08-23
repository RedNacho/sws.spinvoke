using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DelegateToPointerArgumentPreprocessor : IContextualArgumentPreprocessor
	{
		private readonly IContextualArgumentPreprocessor _delegateToUnmanagedFunctionArgumentPreprocessor;

		/// <summary>
		/// Constructor provided for backwards compatibility only. Please inject a delegateToUnmanagedFunctionArgumentPreprocessor.
		/// </summary>
		public DelegateToPointerArgumentPreprocessor() : this(new DelegateToUnmanagedFunctionArgumentPreprocessor()) {
		}

		public DelegateToPointerArgumentPreprocessor(IContextualArgumentPreprocessor delegateToUnmanagedFunctionArgumentPreprocessor) {
			if (delegateToUnmanagedFunctionArgumentPreprocessor == null) {
				throw new ArgumentNullException ("delegateToUnmanagedFunctionArgumentPreprocessor");
			}

			_delegateToUnmanagedFunctionArgumentPreprocessor = delegateToUnmanagedFunctionArgumentPreprocessor;
		}

		public void SetContext (ArgumentPreprocessorContext context)
		{
			_delegateToUnmanagedFunctionArgumentPreprocessor.SetContext (context);
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
			throw new NotImplementedException ();
		}

		protected virtual Delegate MapToInteropCompatibleDelegate (Delegate del) {
			if (_delegateToUnmanagedFunctionArgumentPreprocessor.CanProcess (del)) {
				return (Delegate) _delegateToUnmanagedFunctionArgumentPreprocessor.Process (del);
			}

			return del;
		}

		public static DelegateToUnmanagedFunctionArgumentPreprocessor.IContextCustomisation CreateContextCustomisation(
			IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
			IDelegateTypeProvider delegateTypeProvider,
			CallingConvention? callingConvention = null,
			Action<Delegate> delegateRegistrationAction = null) {
			return DelegateToUnmanagedFunctionArgumentPreprocessor.CreateContextCustomisation (
				delegateTypeToDelegateSignatureConverter,
				delegateTypeProvider,
				callingConvention,
				delegateRegistrationAction
			);
		}
	}
}