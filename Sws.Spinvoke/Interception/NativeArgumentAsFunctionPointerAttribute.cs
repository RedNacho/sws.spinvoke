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

		public static DelegateToPointerArgumentPreprocessor.IContextDecoration CreateContextDecoration(IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
			IDelegateTypeProvider delegateTypeProvider,
			CallingConvention? callingConvention = null,
			Action<Delegate> delegateRegistrationAction = null) {
			return DelegateToPointerArgumentPreprocessor.CreateContextDecoration (
				delegateTypeToDelegateSignatureConverter,
				delegateTypeProvider,
				callingConvention,
				delegateRegistrationAction
			);
		}
	}
}

