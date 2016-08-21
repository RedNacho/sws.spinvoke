using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	public class NativeDelegateInterceptorContext
	{
		private readonly string _libraryName;
		private readonly CallingConvention _callingConvention;
		private readonly INativeDelegateResolver _nativeDelegateResolver;
		private readonly Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext> _argumentPreprocessorContextDecorator;
		private readonly Func<ReturnPostprocessorContext, ReturnPostprocessorContext> _returnPostprocessorContextDecorator;

		public NativeDelegateInterceptorContext(string libraryName, CallingConvention callingConvention, INativeDelegateResolver nativeDelegateResolver,
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

		public string LibraryName { get { return _libraryName; } }

		public CallingConvention CallingConvention { get { return _callingConvention; } }

		public INativeDelegateResolver NativeDelegateResolver { get { return _nativeDelegateResolver; } }

		public Func<ArgumentPreprocessorContext, ArgumentPreprocessorContext> ArgumentPreprocessorContextDecorator { get { return _argumentPreprocessorContextDecorator; } }

		public Func<ReturnPostprocessorContext, ReturnPostprocessorContext> ReturnPostprocessorContextDecorator { get { return _returnPostprocessorContextDecorator; } }
	}
}