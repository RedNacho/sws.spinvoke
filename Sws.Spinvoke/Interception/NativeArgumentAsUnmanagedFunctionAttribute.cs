using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsUnmanagedFunctionAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		public NativeArgumentAsUnmanagedFunctionAttribute ()
			: base(new DelegateToUnmanagedFunctionArgumentPreprocessor(), typeof(Delegate))
		{
		}

		public static DelegateToUnmanagedFunctionArgumentPreprocessor.IContextCustomisation CreateContextCustomisation (IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
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

