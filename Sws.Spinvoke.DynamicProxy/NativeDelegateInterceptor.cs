using System;
using System.Linq;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;

using Castle.DynamicProxy;

namespace Sws.Spinvoke.DynamicProxy
{
	public class NativeDelegateInterceptor : IInterceptor
	{
		private readonly string _libraryName;

		private readonly INativeDelegateResolver _nativeDelegateResolver;

		public NativeDelegateInterceptor(string libraryName, INativeDelegateResolver nativeDelegateResolver)
		{
			if (libraryName == null)
				throw new ArgumentNullException ("libraryName");

			if (nativeDelegateResolver == null)
				throw new ArgumentNullException ("nativeDelegateResolver");

			_libraryName = libraryName;
			_nativeDelegateResolver = nativeDelegateResolver;
		}

		public void Intercept (IInvocation invocation)
		{
			var definitionOverrideAttribute = invocation.Method.GetCustomAttributes (false)
				.Select (attribute => attribute as NativeDelegateDefinitionOverrideAttribute)
				.Where (attribute => attribute != null)
				.DefaultIfEmpty (new NativeDelegateDefinitionOverrideAttribute ())
				.First ();

			var mapNative = definitionOverrideAttribute.MapNative.GetValueOrDefault (true);

			if (!mapNative) {
				throw new NotSupportedException ();
			}

			var libraryName = definitionOverrideAttribute.LibraryName ?? _libraryName;

			var functionName = definitionOverrideAttribute.FunctionName ?? invocation.Method.Name;

			var delegateSignature = definitionOverrideAttribute.DelegateSignature ?? new DelegateSignature (invocation.Method.GetParameters ().Select (parameter => parameter.ParameterType).ToArray (), invocation.Method.ReturnType, CallingConvention.Winapi);

			var delegateInstance = _nativeDelegateResolver.Resolve(new NativeDelegateDefinition(libraryName, functionName, delegateSignature));

			invocation.ReturnValue = delegateInstance.DynamicInvoke (invocation.Arguments);
		}
	}
}

