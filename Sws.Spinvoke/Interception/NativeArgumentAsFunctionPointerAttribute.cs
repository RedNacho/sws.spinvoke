using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsFunctionPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		private readonly DelegateToInteropCompatibleDelegateArgumentPreprocessor _delegateToInteropCompatibleDelegateArgumentPreprocessor;

		public NativeArgumentAsFunctionPointerAttribute () : this(new DelegateToInteropCompatibleDelegateArgumentPreprocessor(null)) {
		}

		protected NativeArgumentAsFunctionPointerAttribute (DelegateToInteropCompatibleDelegateArgumentPreprocessor delegateToInteropCompatibleDelegateArgumentPreprocessor)
			: base(new DelegateToPointerArgumentPreprocessor(
				delegateToInteropCompatibleDelegateArgumentPreprocessor), typeof(Delegate))
		{
			_delegateToInteropCompatibleDelegateArgumentPreprocessor = delegateToInteropCompatibleDelegateArgumentPreprocessor;
		}

		public CallingConvention CallingConvention {
			get {
				return _delegateToInteropCompatibleDelegateArgumentPreprocessor.CallingConvention.Value;
			}
			set {
				_delegateToInteropCompatibleDelegateArgumentPreprocessor.CallingConvention = value;
			}
		}

		public static DelegateToInteropCompatibleDelegateArgumentPreprocessor.IContextCustomisation CreateContextCustomisation (IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
			IDelegateTypeProvider delegateTypeProvider,
			CallingConvention? callingConvention = null,
			Action<Delegate> delegateRegistrationAction = null) {
			return DelegateToInteropCompatibleDelegateArgumentPreprocessor.CreateContextCustomisation (
				delegateTypeToDelegateSignatureConverter,
				delegateTypeProvider,
				callingConvention,
				delegateRegistrationAction
			);
		}
	}
}

