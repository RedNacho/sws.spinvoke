using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Delegates
{
	public class DefaultDelegateTypeToDelegateSignatureConverter : IDelegateTypeToDelegateSignatureConverter
	{
		public DelegateSignature CreateDelegateSignature (Type delegateType, CallingConvention callingConvention)
		{	
			var method = delegateType.GetMethod ("Invoke");

			return new DelegateSignature (method.GetParameters ().Select (parameterInfo => parameterInfo.ParameterType).ToArray (), method.ReturnType, callingConvention);
		}
	}
}

