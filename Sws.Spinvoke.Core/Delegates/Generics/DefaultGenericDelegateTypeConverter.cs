using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Delegates.Generics
{
	public class DefaultGenericDelegateTypeConverter : IGenericDelegateTypeConverter
	{
		private readonly IDelegateTypeProvider _delegateTypeProvider;

		public DefaultGenericDelegateTypeConverter (IDelegateTypeProvider delegateTypeProvider)
		{
			if (delegateTypeProvider == null)
			{
				throw new ArgumentNullException ("delegateTypeProvider");
			}

			_delegateTypeProvider = delegateTypeProvider;
		}

		public Type ConvertToInteropSupportedDelegateType (Type genericDelegateType, CallingConvention callingConvention)
		{
			var method = genericDelegateType.GetMethod ("Invoke");

			var delegateSignature = new DelegateSignature (method.GetParameters ().Select (parameterInfo => parameterInfo.ParameterType).ToArray (), method.ReturnType, callingConvention);

			return _delegateTypeProvider.GetDelegateType (delegateSignature);
		}
	}
}

