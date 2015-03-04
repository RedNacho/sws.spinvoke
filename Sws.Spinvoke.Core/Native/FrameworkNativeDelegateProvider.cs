using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Native
{
	public class FrameworkNativeDelegateProvider : INativeDelegateProvider
	{
		public Delegate GetDelegate (Type delegateType, IntPtr functionPtr)
		{
			return Marshal.GetDelegateForFunctionPointer (functionPtr, delegateType);
		}
	}
}

