using System;
using System.Runtime.Serialization;

namespace Sws.Spinvoke.Core.Exceptions
{
	public class NativeLibraryLoadException : Exception
	{
		public NativeLibraryLoadException ()
		{
		}

		public NativeLibraryLoadException(string message) : base(message)
		{
		}

		public NativeLibraryLoadException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public NativeLibraryLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}

