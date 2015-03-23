using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Delegates.Generics
{
	public class DefaultGenericDelegateTypeConverter : IGenericDelegateTypeConverter
	{
		private readonly IDelegateTypeToDelegateSignatureConverter _delegateTypeToDelegateSignatureConverter;

		private readonly IDelegateTypeProvider _delegateTypeProvider;

		public DefaultGenericDelegateTypeConverter (IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter, IDelegateTypeProvider delegateTypeProvider)
		{
			if (delegateTypeToDelegateSignatureConverter == null) {
				throw new ArgumentNullException ("delegateTypeToDelegateSignatureConverter");
			}

			if (delegateTypeProvider == null) {
				throw new ArgumentNullException ("delegateTypeProvider");
			}

			_delegateTypeToDelegateSignatureConverter = delegateTypeToDelegateSignatureConverter;
			_delegateTypeProvider = delegateTypeProvider;
		}

		public Type ConvertToInteropSupportedDelegateType (Type genericDelegateType, CallingConvention callingConvention)
		{
			var delegateSignature = _delegateTypeToDelegateSignatureConverter.CreateDelegateSignature(genericDelegateType, callingConvention);

			return _delegateTypeProvider.GetDelegateType (delegateSignature);
		}
	}
}

