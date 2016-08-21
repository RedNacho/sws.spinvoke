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

		private readonly Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext> _argumentPreprocessorContextDecorator;

		private readonly Func<ReturnPostprocessorContext, ReturnPostprocessorContext> _returnPostprocessorContextDecorator;

		private readonly object _nativeDelegateMappingsSyncObject = new object ();

		private readonly IDictionary<MethodInfo, NativeDelegateMapping> _nativeDelegateMappings = new Dictionary<MethodInfo, NativeDelegateMapping>();

		public NativeDelegateInterceptor(string libraryName, CallingConvention callingConvention, INativeDelegateResolver nativeDelegateResolver,
			Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext> argumentPreprocessorContextDecorator = null,
			Func<ReturnPostprocessorContext, ReturnPostprocessorContext> returnPostprocessorContextDecorator = null)
		{
			if (libraryName == null)
				throw new ArgumentNullException ("libraryName");

			if (nativeDelegateResolver == null)
				throw new ArgumentNullException ("nativeDelegateResolver");

			_libraryName = libraryName;
			_callingConvention = callingConvention;
			_nativeDelegateResolver = nativeDelegateResolver;
			_argumentPreprocessorContextDecorator = argumentPreprocessorContextDecorator;
			_returnPostprocessorContextDecorator = returnPostprocessorContextDecorator;
		}

		public void Intercept (IInvocation invocation)
		{
			var nativeDelegateMapping = GetNativeDelegateMapping (invocation.Method);

			var mapNative = nativeDelegateMapping.MapNative;

			if (!mapNative) {
				invocation.Proceed ();
				return;
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

			var argumentPreprocessorContexts = new Dictionary<int, ArgumentPreprocessorContext> ();

			try {
				foreach (var pair in Enumerable.Range(0, invocation.Arguments.Length).Zip(argumentPreprocessors, (argIndex, preprocessor) => new { ArgIndex = argIndex, Arg = invocation.Arguments[argIndex], Preprocessor = preprocessor })) {
					SetArgumentPreprocessorContext (pair.Preprocessor, pair.ArgIndex, invocation, nativeDelegateMapping, argumentPreprocessorContexts);

					if (!pair.Preprocessor.CanProcess(pair.Arg)) {
						throw new InvalidOperationException("ArgumentPreprocessor cannot process input.");
					}

					processedArguments.Add(new ProcessedArgument { ArgIndex = pair.ArgIndex, Arg = pair.Preprocessor.Process(pair.Arg), Source = pair.Preprocessor });
				}

				inputTypes = inputTypes.Zip (processedArguments, (inputType, arg) => inputType.IsInstanceOfType (arg.Arg) ? inputType : arg.Arg.GetType ()).ToArray();
			
				var delegateSignature = new DelegateSignature (inputTypes, outputType, callingConvention);

				var delegateInstance = _nativeDelegateResolver.Resolve(new NativeDelegateDefinition(libraryName, functionName, delegateSignature, explicitDelegateType));

				var returnedValue = delegateInstance.DynamicInvoke (processedArguments.Select(arg => arg.Arg).ToArray());

				SetReturnPostprocessorContext (returnPostprocessor, invocation, nativeDelegateMapping, processedArguments, _nativeDelegateResolver, delegateSignature, delegateInstance);

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
						SetArgumentPreprocessorContext (typedArgument.Source, typedArgument.ArgIndex, invocation, nativeDelegateMapping, argumentPreprocessorContexts);

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

		private void SetArgumentPreprocessorContext(IArgumentPreprocessor argumentPreprocessor, int argIndex, IInvocation invocation, NativeDelegateMapping nativeDelegateMapping, IDictionary<int, ArgumentPreprocessorContext> cache)
		{
			var contextualPreprocessor = argumentPreprocessor as IContextualArgumentPreprocessor;

			if (contextualPreprocessor != null) {
				ArgumentPreprocessorContext context;

				if (!cache.ContainsKey (argIndex)) {
					context = new ArgumentPreprocessorContext (invocation, nativeDelegateMapping, argIndex);

					if (_argumentPreprocessorContextDecorator != null) {
						context = _argumentPreprocessorContextDecorator (context);
					}

					cache [argIndex] = context;
				} else {
					context = cache [argIndex];
				}

				contextualPreprocessor.SetContext(context);
			}
		}

		private void SetReturnPostprocessorContext(IReturnPostprocessor returnPostprocessor, IInvocation invocation, NativeDelegateMapping nativeDelegateMapping, IEnumerable<ProcessedArgument> processedArguments, INativeDelegateResolver nativeDelegateResolver, DelegateSignature delegateSignature, Delegate delegateInstance)
		{
			var contextualReturnPostprocessor = returnPostprocessor as IContextualReturnPostprocessor;

			if (contextualReturnPostprocessor != null) {
				var context = new ReturnPostprocessorContext (invocation, nativeDelegateMapping, processedArguments.Select (processedArg => processedArg.Arg).ToArray (), nativeDelegateResolver, delegateSignature, delegateInstance);

				if (_returnPostprocessorContextDecorator != null) {
					context = _returnPostprocessorContextDecorator (context);
				}

				contextualReturnPostprocessor.SetContext (context);
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
					.OfType<NativeDelegateDefinitionOverrideAttribute>()
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
					.OfType<NativeArgumentDefinitionOverrideAttribute>()
					.DefaultIfEmpty(new DefaultNativeArgumentDefinitionOverrideAttribute(inputType))
					.First()).ToArray();

				var returnDefinitionOverrideAttribute = methodInfo.ReturnParameter.GetCustomAttributes()
					.OfType<NativeReturnDefinitionOverrideAttribute>()
					.DefaultIfEmpty(new DefaultNativeReturnDefinitionOverrideAttribute(outputType))
					.First();

				inputTypes = inputTypes.Zip (argumentDefinitionOverrideAttributes, (inputType, attribute) => attribute.InputType ?? inputType).ToArray();

				outputType = returnDefinitionOverrideAttribute.OutputType ?? outputType;

				nativeDelegateMapping = new NativeDelegateMapping (
					mapNative,
					libraryName,
					functionName,
					callingConvention,
					argumentDefinitionOverrideAttributes.Select (adoa => adoa.ArgumentPreprocessor).ToArray (),
					explicitDelegateType,
					inputTypes,
					outputType,
					returnDefinitionOverrideAttribute.ReturnPostprocessor);

				_nativeDelegateMappings [methodInfo] = nativeDelegateMapping;

				return nativeDelegateMapping;
			}
		}

		private class ProcessedArgument
		{
			public int ArgIndex { get; set; }

			public object Arg { get; set; }

			public IArgumentPreprocessor Source { get; set; }
		}
	}
}
