using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsInteropCompatibleDelegateAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsInteropCompatibleDelegateAttribute ()
			: base(new DelegateToInteropCompatibleDelegateArgumentPreprocessor(), typeof(Delegate))
		{
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

