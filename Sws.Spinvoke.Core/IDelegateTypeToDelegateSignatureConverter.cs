using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core
{
	public interface IDelegateTypeToDelegateSignatureConverter
	{
		DelegateSignature CreateDelegateSignature(Type delegateType, CallingConvention callingConvention);
	}
}

