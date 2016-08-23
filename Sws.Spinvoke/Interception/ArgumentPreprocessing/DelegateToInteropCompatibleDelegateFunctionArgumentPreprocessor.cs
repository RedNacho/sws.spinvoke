using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class DelegateToInteropCompatibleDelegateArgumentPreprocessor : IContextualArgumentPreprocessor
	{
		private CallingConvention _methodCallingConvention;

		private IContextCustomisation _contextCustomisation = null;

		public DelegateToInteropCompatibleDelegateArgumentPreprocessor(CallingConvention? callingConvention) {
			this.CallingConvention = callingConvention;
		}

		public CallingConvention? CallingConvention {
			get;
			set;
		}

		public bool CanProcess (object input)
		{
			return input is Delegate && _contextCustomisation != null;
		}

		public object Process (object input)
		{
			return MapToInteropCompatibleDelegate ((Delegate)input);
		}

		public void ReleaseProcessedInput (object processedInput)
		{
		}

		public void SetContext (ArgumentPreprocessorContext context)
		{
			_methodCallingConvention = context.NativeDelegateMapping.CallingConvention;
			_contextCustomisation = ExtractContextCustomisation (context);
		}

		protected virtual Delegate MapToInteropCompatibleDelegate (Delegate del)
		{
			var delegateSignature = _contextCustomisation.DelegateTypeToDelegateSignatureConverter.CreateDelegateSignature (del.GetType (),
				CallingConvention.GetValueOrDefault(
					_contextCustomisation.CallingConvention.GetValueOrDefault (
						_methodCallingConvention)));

			var delegateType = _contextCustomisation.DelegateTypeProvider.GetDelegateType (delegateSignature);

			del = Delegate.CreateDelegate (delegateType, del.Target, del.Method);

			_contextCustomisation.RegisterDelegate (del);

			return del;
		}

		private IContextCustomisation ExtractContextCustomisation(ArgumentPreprocessorContext context) {
			var directCustomisation = context as IContextCustomisation;

			if (directCustomisation != null) {
				return directCustomisation;
			}

			var standardCustomisation = context as ICustomised<IContextCustomisation>;

			if (standardCustomisation != null) {
				return standardCustomisation.Customisation;
			}

			return null;
		}

		public interface IContextCustomisation {
			IDelegateTypeToDelegateSignatureConverter DelegateTypeToDelegateSignatureConverter { get; }
			IDelegateTypeProvider DelegateTypeProvider { get; }
			CallingConvention? CallingConvention { get; }
			void RegisterDelegate (Delegate del);
		}

		private class InternalContextCustomisation : IContextCustomisation {

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

		public static IContextCustomisation CreateContextCustomisation(
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

			return new InternalContextCustomisation {
				DelegateTypeToDelegateSignatureConverter = delegateTypeToDelegateSignatureConverter,
				DelegateTypeProvider = delegateTypeProvider,
				DelegateRegistrationAction = delegateRegistrationAction,
				CallingConvention = callingConvention
			};
		}
	}
}