using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core.Delegates
{
	public class DefaultDelegateTypeToDelegateSignatureConverter : IDelegateTypeToDelegateSignatureConverter
	{
		public bool HasCallingConvention (Type delegateType)
		{
			VerifyDelegateType (delegateType);

			return GetUnmanagedFunctionPointerAttribute(delegateType) != null;
		}

		public DelegateSignature CreateDelegateSignature (Type delegateType)
		{
			VerifyDelegateType (delegateType);

			var unmanagedFunctionPointerAttribute = GetUnmanagedFunctionPointerAttribute (delegateType);
		
			if (unmanagedFunctionPointerAttribute == null) {
				throw new InvalidOperationException ("If you do not specify a calling convention, the UnmanagedFunctionPointerAttribute must be present on the delegate.");
			}

			return CreateDelegateSignature (delegateType, unmanagedFunctionPointerAttribute.CallingConvention);
		}

		public DelegateSignature CreateDelegateSignature (Type delegateType, CallingConvention callingConvention)
		{	
			VerifyDelegateType (delegateType);

			var method = delegateType.GetMethod ("Invoke");

			return new DelegateSignature (method.GetParameters ().Select (parameterInfo => parameterInfo.ParameterType).ToArray (), method.ReturnType, callingConvention);
		}

		private void VerifyDelegateType(Type delegateType)
		{
			if (!typeof(Delegate).IsAssignableFrom(delegateType)) {
				throw new InvalidOperationException("Type must be a delegate.");
			}
		}

		private UnmanagedFunctionPointerAttribute GetUnmanagedFunctionPointerAttribute(Type delegateType)
		{
			return delegateType.GetCustomAttributes (false)
				.OfType<UnmanagedFunctionPointerAttribute>()
				.FirstOrDefault ();
		}
	}
}

