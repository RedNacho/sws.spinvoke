using System;
using Sws.Spinvoke.Core;
using System.Collections;
using System.Linq;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class CustomisedReturnPostprocessorContext<TCustomisation> : ReturnPostprocessorContext, ICustomised<TCustomisation>
		where TCustomisation : class
	{
		private readonly TCustomisation _customisation;

		public CustomisedReturnPostprocessorContext(ReturnPostprocessorContext source,
			TCustomisation customisation
		) : base(source) {
			this._customisation = customisation;
		}

		public TCustomisation Customisation {
			get { return _customisation; }
		}

		public CustomisedReturnPostprocessorContext<IEnumerable> AlsoCustomiseWith<TOtherCustomisation>(TOtherCustomisation customisation)
			where TOtherCustomisation : class
		{
			return new CustomisedReturnPostprocessorContext<IEnumerable>(
				this, CustomisationHelper.CustomisationAsEnumerable(_customisation).Cast<object>()
					.Union<object>(CustomisationHelper.CustomisationAsEnumerable(customisation).Cast<object>())
			);
		}
	}
}

