using System;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class CustomisedArgumentPreprocessorContext<TCustomisation> : ArgumentPreprocessorContext, ICustomised<TCustomisation> {
		private readonly TCustomisation _customisation;

		public CustomisedArgumentPreprocessorContext(ArgumentPreprocessorContext source,
			TCustomisation customisation
		) : base(source) {
			_customisation = customisation;
		}

		public TCustomisation Customisation {
			get { return _customisation; }
		}
	}
}

