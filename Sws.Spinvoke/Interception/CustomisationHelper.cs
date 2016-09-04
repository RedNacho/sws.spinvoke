using System;
using System.Collections;

namespace Sws.Spinvoke.Interception
{
	internal static class CustomisationHelper
	{
		public static TCustomisation ScanForCustomisation<TCustomisation>(object source)
			where TCustomisation : class
		{
			var directCustomisation = source as TCustomisation;

			if (directCustomisation != null) {
				return directCustomisation;
			}

			var standardCustomisation = source as ICustomised<TCustomisation>;

			if (standardCustomisation != null) {
				return standardCustomisation.Customisation;
			}

			var enumerableCustomisation = source as ICustomised<IEnumerable>;

			if (enumerableCustomisation != null) {
				foreach (object element in enumerableCustomisation.Customisation) {
					var customisation = ScanForCustomisation<TCustomisation> (element);

					if (customisation != null) {
						return customisation;
					}
				}
			}

			return null;
		}

		public static IEnumerable CustomisationAsEnumerable<TCustomisation>(TCustomisation customisation)
			where TCustomisation : class
		{
			var enumerableCustomisation = customisation as IEnumerable;

			if (enumerableCustomisation == null) {
				enumerableCustomisation = new object[] { customisation };
			}

			return enumerableCustomisation;
		}
	}
}