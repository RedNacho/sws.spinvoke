using System;
using System.Reflection;

using IDynamicProxyInterceptor = Castle.DynamicProxy.IInterceptor;
using IDynamicProxyInvocation = Castle.DynamicProxy.IInvocation;

using ISpinvokeInterceptor = Sws.Spinvoke.Interception.IInterceptor;
using ISpinvokeInvocation = Sws.Spinvoke.Interception.IInvocation;

namespace Sws.Spinvoke.Interception.DynamicProxy
{
	public class SpinvokeInterceptor : IDynamicProxyInterceptor
	{
		private readonly ISpinvokeInterceptor _interceptor;

		public SpinvokeInterceptor(ISpinvokeInterceptor interceptor)
		{
			if (interceptor == null) {
				throw new ArgumentNullException ("interceptor");
			}

			_interceptor = interceptor;
		}

		public void Intercept (IDynamicProxyInvocation invocation)
		{
			_interceptor.Intercept (new SpinvokeInvocation (invocation));
		}

		private class SpinvokeInvocation : ISpinvokeInvocation
		{
			private readonly IDynamicProxyInvocation _invocation;

			public SpinvokeInvocation(IDynamicProxyInvocation invocation)
			{
				if (invocation == null) {
					throw new ArgumentNullException("invocation");
				}

				_invocation = invocation;
			}

			public object[] Arguments {
				get {
					return _invocation.Arguments;
				}
			}

			public MethodInfo Method {
				get {
					return _invocation.Method;
				}
			}

			public object ReturnValue {
				get {
					return _invocation.ReturnValue;
				}
				set {
					_invocation.ReturnValue = value;
				}
			}
		}
	}
}

