using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core
{
	public interface IDelegateTypeToDelegateSignatureConverter
	{
		bool HasCallingConvention(Type delegateType);
		DelegateSignature CreateDelegateSignature(Type delegateType);
		DelegateSignature CreateDelegateSignature(Type delegateType, CallingConvention callingConvention);
	}
}

