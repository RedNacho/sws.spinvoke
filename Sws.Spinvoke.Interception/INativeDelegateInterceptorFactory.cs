using System;

namespace Sws.Spinvoke.Interception
{
	public interface INativeDelegateInterceptorFactory
	{
		IInterceptor CreateInterceptor(NativeDelegateInterceptorContext context);
	}
}

