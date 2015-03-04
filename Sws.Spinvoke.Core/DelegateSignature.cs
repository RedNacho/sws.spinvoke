using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core
{
	public class DelegateSignature
	{
		private readonly Type[] _inputTypes;
		private readonly Type _outputType;
		private readonly CallingConvention _callingConvention;

		public DelegateSignature(Type[] inputTypes, Type outputType, CallingConvention callingConvention)
		{
			if (inputTypes == null) throw new ArgumentNullException("inputTypes");
			if (outputType == null) throw new ArgumentNullException("outputType");

			_inputTypes = inputTypes;
			_outputType = outputType;
			_callingConvention = callingConvention;
		}

		public Type[] InputTypes { get { return _inputTypes; } }
		public Type OutputType { get { return _outputType; } }
		public CallingConvention CallingConvention { get { return _callingConvention; } }

		public CacheKey GetCacheKey()
		{
			var callingConvention = CallingConvention;
			var inputTypes = InputTypes;
			var outputType = OutputType;

			return new CacheKey.Builder ()
				.AddComponent (callingConvention)
				.AddComponent (outputType)
				.AddComponents (inputTypes).Build ();
		}
	}
}

