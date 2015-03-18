using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;

namespace Sws.Spinvoke.Interception
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

			var mapNative = definitionOverrideAttribute.MapNativeNullable.GetValueOrDefault (true);

			if (!mapNative) {
				throw new NotSupportedException ();
			}

			var libraryName = definitionOverrideAttribute.LibraryName ?? _libraryName;

			var functionName = definitionOverrideAttribute.FunctionName ?? invocation.Method.Name;

			var parameters = invocation.Method.GetParameters ();

			var inputTypes = definitionOverrideAttribute.InputTypes ?? parameters.Select (parameter => parameter.ParameterType).ToArray ();

			var outputType = definitionOverrideAttribute.OutputType ?? invocation.Method.ReturnType;

			var callingConvention = definitionOverrideAttribute.CallingConventionNullable.GetValueOrDefault (_callingConvention);

			var argumentDefinitionOverrideAttributes = parameters.Zip(inputTypes, (parameter, inputType) =>
				parameter.GetCustomAttributes()
					.Select(attribute => attribute as NativeArgumentDefinitionOverrideAttribute)
					.Where(attribute => attribute != null)
					.DefaultIfEmpty(new DefaultNativeArgumentDefinitionOverrideAttribute(inputType))
					.First());

			var returnDefinitionOverrideAttribute = invocation.Method.ReturnParameter.GetCustomAttributes()
				.Select(attribute => attribute as NativeReturnDefinitionOverrideAttribute)
				.Where(attribute => attribute != null)
				.DefaultIfEmpty(new DefaultNativeReturnDefinitionOverrideAttribute(outputType))
				.First();

			inputTypes = inputTypes.Zip (argumentDefinitionOverrideAttributes, (inputType, attribute) => attribute.InputType ?? inputType).ToArray();

			outputType = returnDefinitionOverrideAttribute.OutputType ?? outputType;

			var processedArguments = new List<ProcessedArgument> ();

			List<Exception> exceptionList = new List<Exception> ();

			try {
				foreach (var pair in invocation.Arguments.Zip(argumentDefinitionOverrideAttributes, (arg, attribute) => new { Arg = arg, Attribute = attribute })) {
					if (!pair.Attribute.ArgumentPreprocessor.CanProcess(pair.Arg)) {
						throw new InvalidOperationException("ArgumentPreprocessor cannot process input.");
					}

					processedArguments.Add(new ProcessedArgument { Arg = pair.Attribute.ArgumentPreprocessor.Process(pair.Arg), Source = pair.Attribute.ArgumentPreprocessor });
				}

				inputTypes = inputTypes.Zip (processedArguments, (inputType, arg) => inputType.IsInstanceOfType (arg.Arg) ? inputType : arg.Arg.GetType ()).ToArray();
			
				var delegateSignature = new DelegateSignature (inputTypes, outputType, callingConvention);

				var delegateInstance = _nativeDelegateResolver.Resolve(new NativeDelegateDefinition(libraryName, functionName, delegateSignature));

				var returnPostprocessor = returnDefinitionOverrideAttribute.ReturnPostprocessor;

				var returnedValue = delegateInstance.DynamicInvoke (processedArguments.Select(arg => arg.Arg).ToArray());

				if (!returnPostprocessor.CanProcess(returnedValue, invocation.Method.ReturnType))
				{
					throw new InvalidOperationException("ReturnPostprocessor cannot convert returned value to required type.");
				}

				invocation.ReturnValue = returnPostprocessor.Process(returnedValue, invocation.Method.ReturnType);
			}
			catch (Exception ex) {
				exceptionList.Add (ex);
			}
			finally {
				foreach (var typedArgument in processedArguments) {
					try {
						typedArgument.Source.ReleaseProcessedInput (typedArgument.Arg);
					}
					catch (Exception ex) {
						exceptionList.Add(ex);
					}
				}
			}

			var exceptionCount = exceptionList.Count;

			if (exceptionCount == 0) {
				return;
			} else if (exceptionCount == 1) {
				throw exceptionList.Single ();
			} else {
				throw new AggregateException (exceptionList);
			}
		}

		private class ProcessedArgument
		{
			public object Arg { get; set; }

			public IArgumentPreprocessor Source { get; set; }
		}
	}
}
