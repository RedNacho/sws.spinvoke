
namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class ArgumentPreprocessorContext
	{
		private readonly IInvocation _invocation;
		private readonly NativeDelegateMapping _nativeDelegateMapping;
		private readonly int _argumentIndex;

		public ArgumentPreprocessorContext (IInvocation invocation,
			NativeDelegateMapping nativeDelegateMapping, int argumentIndex)
		{
			_invocation = invocation;
			_nativeDelegateMapping = nativeDelegateMapping;
			_argumentIndex = argumentIndex;
		}

		protected ArgumentPreprocessorContext (ArgumentPreprocessorContext source)
			: this(source.Invocation, source.NativeDelegateMapping, source.ArgumentIndex)
		{
		}

		public IInvocation Invocation { get { return _invocation; } }
		public NativeDelegateMapping NativeDelegateMapping { get { return _nativeDelegateMapping; } }
		public int ArgumentIndex { get { return _argumentIndex; } }

		public CustomisedArgumentPreprocessorContext<TCustomisation> Customise<TCustomisation>(TCustomisation customisation)
			where TCustomisation : class
		{
			return new CustomisedArgumentPreprocessorContext<TCustomisation> (this, customisation);
		}

		public TCustomisation ScanForCustomisation<TCustomisation>()
			where TCustomisation : class
		{
			return CustomisationHelper.ScanForCustomisation<TCustomisation> (this);
		}
	}
}

