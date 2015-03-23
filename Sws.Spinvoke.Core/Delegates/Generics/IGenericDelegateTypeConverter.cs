using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Delegates.Generics
{
	public interface IGenericDelegateTypeConverter
	{
		Type ConvertToInteropSupportedDelegateType(Type genericDelegateType, CallingConvention callingConvention);
	}
}

