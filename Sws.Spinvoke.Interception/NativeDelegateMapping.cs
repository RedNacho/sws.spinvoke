using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	public class NativeDelegateMapping
	{
		private readonly bool _mapNative;
		private readonly string _libraryName;
		private readonly string _functionName;
		private readonly CallingConvention _callingConvention;
		private readonly IArgumentPreprocessor[] _argumentPreprocessors;
		private readonly Type _explicitDelegateType;
		private readonly Type[] _inputTypes;
		private readonly Type _outputType;
		private readonly IReturnPostprocessor _returnPostprocessor;

		internal NativeDelegateMapping(bool mapNative,
			string libraryName,
			string functionName,
			CallingConvention callingConvention,
			IArgumentPreprocessor[] argumentPreprocessors,
			Type explicitDelegateType,
			Type[] inputTypes,
			Type outputType,
			IReturnPostprocessor returnPostprocessor)
		{
			_mapNative = mapNative;
			_libraryName = libraryName;
			_functionName = functionName;
			_callingConvention = callingConvention;
			_argumentPreprocessors = argumentPreprocessors;
			_explicitDelegateType = explicitDelegateType;
			_inputTypes = inputTypes;
			_outputType = outputType;
			_returnPostprocessor = returnPostprocessor;
		}

		public bool MapNative { get { return _mapNative; } }

		public string LibraryName { get { return _libraryName; } }

		public string FunctionName { get { return _functionName; } }

		public CallingConvention CallingConvention { get { return _callingConvention; } }

		public IArgumentPreprocessor[] ArgumentPreprocessors { get { return _argumentPreprocessors; } }

		public Type ExplicitDelegateType { get { return _explicitDelegateType; } }

		public Type[] InputTypes { get { return _inputTypes; } }

		public Type OutputType { get { return _outputType; } }

		public IReturnPostprocessor ReturnPostprocessor { get { return _returnPostprocessor; } }
	}
}

