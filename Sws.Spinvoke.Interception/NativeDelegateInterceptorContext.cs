using System;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception
{
	public class NativeDelegateInterceptorContext
	{
		private readonly string _libraryName;
		private readonly CallingConvention _callingConvention;
		private readonly INativeDelegateResolver _nativeDelegateResolver;

		public NativeDelegateInterceptorContext(string libraryName, CallingConvention callingConvention, INativeDelegateResolver nativeDelegateResolver)
		{
			if (libraryName == null)
				throw new ArgumentNullException ("libraryName");

			if (nativeDelegateResolver == null)
				throw new ArgumentNullException ("nativeDelegateResolver");

			_libraryName = libraryName;
			_callingConvention = callingConvention;
			_nativeDelegateResolver = nativeDelegateResolver;
		}

		public string LibraryName { get { return _libraryName; } }

		public CallingConvention CallingConvention { get { return _callingConvention; } }

		public INativeDelegateResolver NativeDelegateResolver { get { return _nativeDelegateResolver; } }
	}
}

