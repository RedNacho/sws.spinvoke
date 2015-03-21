using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	public class NativeDelegateInterceptor : IInterceptor
	{
		private readonly string _libraryName;

		private readonly CallingConvention _callingConvention;

		private readonly INativeDelegateResolver _nativeDelegateResolver;

		private readonly object _nativeDelegateMappingsSyncObject = new object ();

		private readonly IDictionary<MethodInfo, NativeDelegateMapping> _nativeDelegateMappings = new Dictionary<MethodInfo, NativeDelegateMapping>();

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
			var nativeDelegateMapping = GetNativeDelegateMapping (invocation.Method);

			var mapNative = nativeDelegateMapping.MapNative;

			if (!mapNative) {
				throw new NotSupportedException ();
			}

			var libraryName = nativeDelegateMapping.LibraryName;
			var functionName = nativeDelegateMapping.FunctionName;
			var callingConvention = nativeDelegateMapping.CallingConvention;
			var argumentPreprocessors = nativeDelegateMapping.ArgumentPreprocessors;
			var inputTypes = nativeDelegateMapping.InputTypes;
			var outputType = nativeDelegateMapping.OutputType;
			var explicitDelegateType = nativeDelegateMapping.ExplicitDelegateType;
			var returnPostprocessor = nativeDelegateMapping.ReturnPostprocessor;

			var processedArguments = new List<ProcessedArgument> ();

			List<Exception> exceptionList = new List<Exception> ();

			try {
				foreach (var pair in invocation.Arguments.Zip(argumentPreprocessors, (arg, preprocessor) => new { Arg = arg, Preprocessor = preprocessor })) {
					if (!pair.Preprocessor.CanProcess(pair.Arg)) {
						throw new InvalidOperationException("ArgumentPreprocessor cannot process input.");
					}

					processedArguments.Add(new ProcessedArgument { Arg = pair.Preprocessor.Process(pair.Arg), Source = pair.Preprocessor });
				}

				inputTypes = inputTypes.Zip (processedArguments, (inputType, arg) => inputType.IsInstanceOfType (arg.Arg) ? inputType : arg.Arg.GetType ()).ToArray();
			
				var delegateSignature = new DelegateSignature (inputTypes, outputType, callingConvention);

				var delegateInstance = _nativeDelegateResolver.Resolve(new NativeDelegateDefinition(libraryName, functionName, delegateSignature, explicitDelegateType));

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

		private NativeDelegateMapping GetNativeDelegateMapping(MethodInfo methodInfo)
		{
			lock (_nativeDelegateMappingsSyncObject) {
				NativeDelegateMapping nativeDelegateMapping;

				if (_nativeDelegateMappings.TryGetValue (methodInfo, out nativeDelegateMapping)) {
					return nativeDelegateMapping;
				}

				var definitionOverrideAttribute = methodInfo.GetCustomAttributes (false)
					.Select (attribute => attribute as NativeDelegateDefinitionOverrideAttribute)
					.Where (attribute => attribute != null)
					.DefaultIfEmpty (new NativeDelegateDefinitionOverrideAttribute ())
					.First ();

				var mapNative = definitionOverrideAttribute.MapNativeNullable.GetValueOrDefault (true);

				var libraryName = definitionOverrideAttribute.LibraryName ?? _libraryName;

				var functionName = definitionOverrideAttribute.FunctionName ?? methodInfo.Name;

				var parameters = methodInfo.GetParameters ();

				var inputTypes = definitionOverrideAttribute.InputTypes ?? parameters.Select (parameter => parameter.ParameterType).ToArray ();

				var outputType = definitionOverrideAttribute.OutputType ?? methodInfo.ReturnType;

				var callingConvention = definitionOverrideAttribute.CallingConventionNullable.GetValueOrDefault (_callingConvention);

				var explicitDelegateType = definitionOverrideAttribute.ExplicitDelegateType;

				var argumentDefinitionOverrideAttributes = parameters.Zip(inputTypes, (parameter, inputType) =>
					parameter.GetCustomAttributes()
					.Select(attribute => attribute as NativeArgumentDefinitionOverrideAttribute)
					.Where(attribute => attribute != null)
					.DefaultIfEmpty(new DefaultNativeArgumentDefinitionOverrideAttribute(inputType))
					.First()).ToArray();

				var returnDefinitionOverrideAttribute = methodInfo.ReturnParameter.GetCustomAttributes()
					.Select(attribute => attribute as NativeReturnDefinitionOverrideAttribute)
					.Where(attribute => attribute != null)
					.DefaultIfEmpty(new DefaultNativeReturnDefinitionOverrideAttribute(outputType))
					.First();

				inputTypes = inputTypes.Zip (argumentDefinitionOverrideAttributes, (inputType, attribute) => attribute.InputType ?? inputType).ToArray();

				outputType = returnDefinitionOverrideAttribute.OutputType ?? outputType;

				nativeDelegateMapping = new NativeDelegateMapping {
					MapNative = mapNative,
					LibraryName = libraryName,
					FunctionName = functionName,
					CallingConvention = callingConvention,
					ArgumentPreprocessors = argumentDefinitionOverrideAttributes.Select(adoa => adoa.ArgumentPreprocessor).ToArray(),
					ExplicitDelegateType = explicitDelegateType,
					InputTypes = inputTypes,
					OutputType = outputType,
					ReturnPostprocessor = returnDefinitionOverrideAttribute.ReturnPostprocessor
				};

				_nativeDelegateMappings [methodInfo] = nativeDelegateMapping;

				return nativeDelegateMapping;
			}
		}

		private class NativeDelegateMapping
		{
			public bool MapNative { get; set;}

			public string LibraryName { get; set; }

			public string FunctionName { get; set;}

			public CallingConvention CallingConvention { get; set; }

			public IArgumentPreprocessor[] ArgumentPreprocessors { get; set; }

			public Type ExplicitDelegateType { get; set; }

			public Type[] InputTypes { get; set; }

			public Type OutputType { get; set; }

			public IReturnPostprocessor ReturnPostprocessor { get; set;}
		}

		private class ProcessedArgument
		{
			public object Arg { get; set; }

			public IArgumentPreprocessor Source { get; set; }
		}
	}
}
