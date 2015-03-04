using System;

namespace Sws.Spinvoke.Core
{
	public interface INativeDelegateProvider
	{
		Delegate GetDelegate(Type delegateType, IntPtr functionPtr);
	}
}

