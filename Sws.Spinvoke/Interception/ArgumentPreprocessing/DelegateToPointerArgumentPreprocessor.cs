using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;
using System.Collections.Generic;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DelegateToPointerArgumentPreprocessor : IContextualArgumentPreprocessor
	{
		private IContextDecoration _contextDecoration = null;

		private HashSet<object> _internalReferences = new HashSet<object> ();

		public bool CanProcess (object input)
		{
			return input is Delegate;
		}

		public object Process (object input)
		{
			Delegate mappedInput = MapToInteropCompatibleDelegate ((Delegate)input);

			return Marshal.GetFunctionPointerForDelegate (mappedInput);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
			_internalReferences.Remove (processedInput);
		}

		public void SetContext (ArgumentPreprocessorContext context)
		{
			_contextDecoration = ExtractContextDecoration (context);
		}

		protected virtual Delegate MapToInteropCompatibleDelegate (Delegate del)
		{
			if (_contextDecoration != null) {
				DelegateSignature delegateSignature;

				if (_contextDecoration.CallingConvention.HasValue) {
					delegateSignature = _contextDecoration.DelegateTypeToDelegateSignatureConverter.CreateDelegateSignature (del.GetType (), _contextDecoration.CallingConvention.Value);
				} else {
					delegateSignature = _contextDecoration.DelegateTypeToDelegateSignatureConverter.CreateDelegateSignature (del.GetType ());
				}

				var delegateType = _contextDecoration.DelegateTypeProvider.GetDelegateType (delegateSignature);

				del = Delegate.CreateDelegate (delegateType, del.Target, del.Method);

				_contextDecoration.RegisterDelegate (del);
			}

			_internalReferences.Add (del);

			return del;
		}

		private IContextDecoration ExtractContextDecoration(ArgumentPreprocessorContext context) {
			var customDecoration = context as IContextDecoration;

			if (customDecoration != null) {
				return customDecoration;
			}
		
			var standardDecoration = context as IDecorated<IContextDecoration>;

			if (standardDecoration != null) {
				return standardDecoration.Decoration;
			}

			return null;
		}

		public interface IContextDecoration {
			IDelegateTypeToDelegateSignatureConverter DelegateTypeToDelegateSignatureConverter { get; }
			IDelegateTypeProvider DelegateTypeProvider { get; }
			CallingConvention? CallingConvention { get; }
			void RegisterDelegate (Delegate del);
		}

		private class InternalContextDecoration : IContextDecoration {
			
			public IDelegateTypeToDelegateSignatureConverter DelegateTypeToDelegateSignatureConverter {
				get;
				set;
			}			

			public IDelegateTypeProvider DelegateTypeProvider {
				get;
				set;
			}

			public CallingConvention? CallingConvention {
				get;
				set;
			}

			public Action<Delegate> DelegateRegistrationAction {
				get;
				set;
			}

			public void RegisterDelegate (Delegate del) {
				if (DelegateRegistrationAction != null) { 
					DelegateRegistrationAction (del);
				}
			}
		}

		public static IContextDecoration CreateContextDecoration(
			IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
			IDelegateTypeProvider delegateTypeProvider,
			CallingConvention? callingConvention = null,
			Action<Delegate> delegateRegistrationAction = null) {
	
			if (delegateTypeToDelegateSignatureConverter == null) {
				throw new ArgumentNullException ("delegateTypeToDelegateSignatureConverter");
			}

			if (delegateTypeProvider == null) {
				throw new ArgumentNullException ("delegateTypeProvider");
			}
			return new InternalContextDecoration {
				DelegateTypeToDelegateSignatureConverter = delegateTypeToDelegateSignatureConverter,
				DelegateTypeProvider = delegateTypeProvider,
				DelegateRegistrationAction = delegateRegistrationAction,
				CallingConvention = callingConvention
			};
		}
	}
}