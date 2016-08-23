using System;

using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Core;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NativeArgumentAsInteropCompatibleDelegateAttribute : NativeArgumentDefinitionOverrideAttribute
	{
		private DelegateToInteropCompatibleDelegateArgumentPreprocessor _delegateToInteropCompatibleDelegateArgumentPreprocessor;

		public NativeArgumentAsInteropCompatibleDelegateAttribute () : this(new DelegateToInteropCompatibleDelegateArgumentPreprocessor(null)) {
		}

		protected NativeArgumentAsInteropCompatibleDelegateAttribute (DelegateToInteropCompatibleDelegateArgumentPreprocessor delegateToInteropCompatibleDelegateArgumentPreprocessor)
			: base(delegateToInteropCompatibleDelegateArgumentPreprocessor, typeof(Delegate))
		{
			_delegateToInteropCompatibleDelegateArgumentPreprocessor = delegateToInteropCompatibleDelegateArgumentPreprocessor;
		}

		public CallingConvention CallingConvention {
			get {
				return _delegateToInteropCompatibleDelegateArgumentPreprocessor.CallingConvention.Value;
			}
			set {
				_delegateToInteropCompatibleDelegateArgumentPreprocessor.CallingConvention = value;
			}
		}

		/// <summary>
		/// This allows you to create a customisation for ArgumentPreprocessorContext with which
		/// this attribute can work. If you override the ArgumentPreprocessorContextCustomiser
		/// passed to the NativeDelegateInterceptor so that it either implements this interface
		/// directly, or is customised via the .Customise method, this enables you to inject
		/// runtime dependencies and settings required for the interop-compatible delegate conversion.
		/// </summary>
		/// <returns>The context customisation.</returns>
		/// <param name="delegateTypeToDelegateSignatureConverter">Required; see Ninject SpinvokeModule for example</param>
		/// <param name="delegateTypeProvider">Required; see Ninject SpinvokeModule for example</param>
		/// <param name="callingConvention">The calling convention to use when creating the new delegate. If not supplied: The value explicitly passed the attribute takes precedence, and the fallback is then the calling convention of the method itself. This should normally be correct, so this parameter will normally be null.</param>
		/// <param name="delegateRegistrationAction">Use this to register the created delegate so that it will not be garbage-collected too soon. By default, the delegate only survives for the duration of the native method call.</param>
		public static DelegateToInteropCompatibleDelegateArgumentPreprocessor.IContextCustomisation CreateContextCustomisation (
			IDelegateTypeToDelegateSignatureConverter delegateTypeToDelegateSignatureConverter,
			IDelegateTypeProvider delegateTypeProvider,
			CallingConvention? callingConvention = null,
			Action<Delegate> delegateRegistrationAction = null) {
			return DelegateToInteropCompatibleDelegateArgumentPreprocessor.CreateContextCustomisation (
				delegateTypeToDelegateSignatureConverter,
				delegateTypeProvider,
				callingConvention,
				delegateRegistrationAction
			);
		}
	}
}

