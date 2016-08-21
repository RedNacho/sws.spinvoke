using System;

namespace Sws.Spinvoke.Interception
{
	public class DefaultNativeDelegateInterceptorFactory : INativeDelegateInterceptorFactory
	{
		public IInterceptor CreateInterceptor (NativeDelegateInterceptorContext context)
		{
			return new NativeDelegateInterceptor (context.LibraryName,
				context.CallingConvention,
				context.NativeDelegateResolver,
				context.ArgumentPreprocessorContextCustomiser,
				context.ReturnPostprocessorContextCustomiser);
		}
	}
}

