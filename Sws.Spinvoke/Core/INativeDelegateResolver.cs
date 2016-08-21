using System;

namespace Sws.Spinvoke.Core
{
	public interface INativeDelegateResolver : IDisposable
	{
		Delegate Resolve(NativeDelegateDefinition nativeDelegateDefinition);
		void Release(Delegate nativeDelegate);
	}
}

