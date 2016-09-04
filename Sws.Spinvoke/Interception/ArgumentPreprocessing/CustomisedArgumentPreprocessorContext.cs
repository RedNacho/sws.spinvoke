using System;
using System.Collections;
using System.Linq;

namespace Sws.Spinvoke.Interception.ArgumentPreprocessing
{
	public class CustomisedArgumentPreprocessorContext<TCustomisation> : ArgumentPreprocessorContext, ICustomised<TCustomisation>
		where TCustomisation : class
	{
		private readonly TCustomisation _customisation;

		public CustomisedArgumentPreprocessorContext(ArgumentPreprocessorContext source,
			TCustomisation customisation
		) : base(source) {
			_customisation = customisation;
		}

		public TCustomisation Customisation {
			get { return _customisation; }
		}

		public CustomisedArgumentPreprocessorContext<IEnumerable> AlsoCustomiseWith<TOtherCustomisation>(TOtherCustomisation customisation) 
			where TOtherCustomisation : class
		{
			return new CustomisedArgumentPreprocessorContext<IEnumerable>(
				this, CustomisationHelper.CustomisationAsEnumerable(_customisation).Cast<object>()
					.Union(CustomisationHelper.CustomisationAsEnumerable(customisation).Cast<object>())
			);
		}
	}
}

