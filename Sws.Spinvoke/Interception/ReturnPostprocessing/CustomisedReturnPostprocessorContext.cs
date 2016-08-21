using System;
using Sws.Spinvoke.Core;

namespace Sws.Spinvoke.Interception.ReturnPostprocessing
{
	public class CustomisedReturnPostprocessorContext<TCustomisation> : ReturnPostprocessorContext, ICustomised<TCustomisation> {
		private readonly TCustomisation _customisation;

		public CustomisedReturnPostprocessorContext(ReturnPostprocessorContext source,
			TCustomisation customisation
		) : base(source) {
			this._customisation = customisation;
		}

		public TCustomisation Customisation {
			get { return _customisation; }
		}
	}
}

