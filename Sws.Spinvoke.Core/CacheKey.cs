using System;
using System.Collections.Generic;
using System.Linq;

namespace Sws.Spinvoke.Core
{
	public class CacheKey : IEquatable<CacheKey>
	{
		private readonly object[] _components;

		public object[] Components { get { return _components; } }

		public CacheKey(params object[] components)
		{
			if (components == null) {
				throw new ArgumentNullException ("components");
			}

			_components = components;
		}

		public bool Equals (CacheKey other)
		{
			if (other == null) {
				return false;
			}

			return Components.SequenceEqual (other.Components);
		}

		public override bool Equals (object obj)
		{
			if (obj == null) {
				return false;
			}

			var cacheKeyObj = obj as CacheKey;

			if (cacheKeyObj == null) {
				return false;
			}

			return Equals (cacheKeyObj);
		}

		public override int GetHashCode ()
		{
			unchecked {
				return Components.Aggregate (0, (accumulate, component) => accumulate = (accumulate * 397) ^ (component == null ? 0 : component.GetHashCode ()));
			}
		}

		public class Builder
		{
			private IList<IEnumerable<object>> _components = new List<IEnumerable<object>>();

			public Builder AddComponent(object component)
			{
				return AddComponents (new [] { component });
			}

			public Builder AddComponents(IEnumerable<object> components)
			{
				_components.Add (components);

				return this;
			}

			public Builder AddComponents(params object[] components)
			{
				return AddComponents ((IEnumerable<object>)components);
			}

			public CacheKey Build()
			{
				return new CacheKey (_components.SelectMany (components => components).ToArray());
			}
		}
	}
}

