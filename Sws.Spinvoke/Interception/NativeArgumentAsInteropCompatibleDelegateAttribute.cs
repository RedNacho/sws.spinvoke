using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsInteropCompatibleDelegateAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		private DelegateToInteropCompatibleDelegateArgumentPreprocessor _delegateToInteropCompatibleDelegateArgumentPreprocessor;

		public NativeArgumentAsInteropCompatibleDelegateAttribute () : this(new DelegateToInteropCompatibleDelegateArgumentPreprocessor(null)) {
		}

		protected NativeArgumentAsInteropCompatibleDelegateAttribute (DelegateToInteropCompatibleDelegateArgumentPreprocessor delegateToInteropCompatibleDelegateArgumentPreprocessor)
			: base(delegateToInteropCompatibleDelegateArgumentPreprocessor, typeof(Delegate))
		{
			_delegateToInteropCompatibleDelegateArgumentPreprocessor = delegateToInteropCompatibleDelegateArgumentPreprocessor;
		}

		public CallingConvention? CallingConvention {
			get {
				return _delegateToInteropCompatibleDelegateArgumentPreprocessor.CallingConvention;
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

