using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core
{
	public interface IDelegateTypeProvider
	{
		Type GetDelegateType(DelegateSignature delegateSignature);
	}
}

