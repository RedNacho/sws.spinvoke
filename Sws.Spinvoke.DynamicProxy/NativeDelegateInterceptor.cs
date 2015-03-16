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

		private readonly CallingConvention _callingConvention;

		private readonly INativeDelegateResolver _nativeDelegateResolver;

		public NativeDelegateInterceptor(string libraryName, CallingConvention callingConvention, INativeDelegateResolver nativeDelegateResolver)
		{
			if (libraryName == null)
				throw new ArgumentNullException ("libraryName");

			if (nativeDelegateResolver == null)
				throw new ArgumentNullException ("nativeDelegateResolver");

			_libraryName = libraryName;
			_callingConvention = callingConvention;
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

			var inputTypes = definitionOverrideAttribute.InputTypes ?? invocation.Method.GetParameters ().Select (parameter => parameter.ParameterType).ToArray ();

			var outputType = definitionOverrideAttribute.OutputType ?? invocation.Method.ReturnType;

			var callingConvention = definitionOverrideAttribute.CallingConvention.GetValueOrDefault (_callingConvention);

			var delegateSignature = new DelegateSignature (inputTypes, outputType, callingConvention);

			var delegateInstance = _nativeDelegateResolver.Resolve(new NativeDelegateDefinition(libraryName, functionName, delegateSignature));

			var argumentPreprocessors = inputTypes.Select (inputType => new ChangeTypeArgumentPreprocessor (inputType));

			var typedArguments = invocation.Arguments.Zip (argumentPreprocessors, (arg, argPreprocessor) => Tuple.Create(argPreprocessor, argPreprocessor.Process(arg))).ToArray();

			var returnPostprocessor = new ChangeTypeReturnPostprocessor();

			try {
				invocation.ReturnValue = returnPostprocessor.Process(delegateInstance.DynamicInvoke (typedArguments.Select(arg => arg.Item2).ToArray()), invocation.Method.ReturnType);
			}
			finally {
				foreach (var typedArgument in typedArguments) {
					typedArgument.Item1.Dispose (typedArgument.Item2);
				}
			}

		}
	}
}
