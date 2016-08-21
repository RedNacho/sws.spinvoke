using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	public class NativeArgumentAsFunctionPointerAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsFunctionPointerAttribute ()
			: base(new DelegateToPointerArgumentPreprocessor(), typeof(Delegate))
		{
		}

		public static DelegateToPointerArgumentPreprocessor.IContextCustomisation CreateContextCustomisation (IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
			IDelegateTypeProvider delegateTypeProvider,
			CallingConvention? callingConvention = null,
			Action<Delegate> delegateRegistrationAction = null) {
			return DelegateToPointerArgumentPreprocessor.CreateContextCustomisation (
				delegateTypeToDelegateSignatureConverter,
				delegateTypeProvider,
				callingConvention,
				delegateRegistrationAction
			);
		}
	}
}

