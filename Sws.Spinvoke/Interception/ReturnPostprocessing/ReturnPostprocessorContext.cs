using System;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class ReturnPostprocessorContext
	{
		private readonly IInvocation _invocation;
		private readonly NativeDelegateMapping _nativeDelegateMapping;
		private readonly object[] _processedArguments;
		private readonly INativeDelegateResolver _nativeDelegateResolver;
		private readonly DelegateSignature _delegateSignature;
		private readonly Delegate _delegateInstance;

		public ReturnPostprocessorContext (IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping,
			object[] processedArguments,
			INativeDelegateResolver nativeDelegateResolver,
			DelegateSignature delegateSignature,
			Delegate delegateInstance)
		{
			_invocation = invocation;
			_nativeDelegateMapping = nativeDelegateMapping;
			_processedArguments = processedArguments;
			_nativeDelegateResolver = nativeDelegateResolver;
			_delegateSignature = delegateSignature;
			_delegateInstance = delegateInstance;
		}

		protected ReturnPostprocessorContext(ReturnPostprocessorContext source)
			: this(source.Invocation, source.NativeDelegateMapping, source.ProcessedArguments,
				source.NativeDelegateResolver, source.DelegateSignature, source.DelegateInstance)
		{
		}

		public IInvocation Invocation {
			get { return _invocation; }
		}

		public NativeDelegateMapping NativeDelegateMapping {
			get { return _nativeDelegateMapping; }
		}

		public object[] ProcessedArguments {
			get { return _processedArguments; }
		}

		public INativeDelegateResolver NativeDelegateResolver {
			get { return _nativeDelegateResolver; }
		}

		public DelegateSignature DelegateSignature {
			get { return _delegateSignature; }
		}

		public Delegate DelegateInstance {
			get { return _delegateInstance; }
		}

		public CustomisedReturnPostprocessorContext<TCustomisation> Customise<TCustomisation>(TCustomisation customisation)
			where TCustomisation : class
		{
			return new CustomisedReturnPostprocessorContext<TCustomisation> (this, customisation);
		}

		public TCustomisation ScanForCustomisation<TCustomisation>()
			where TCustomisation : class
		{
			return CustomisationHelper.ScanForCustomisation<TCustomisation> (this);
		}
	}
}

