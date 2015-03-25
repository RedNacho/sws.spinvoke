using System;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Core
{
	public abstract class SafeLibraryHandle : SafeHandle
	{
		protected SafeLibraryHandle () :
			base (IntPtr.Zero, true)
		{
		}

		public sealed override bool IsInvalid {
			get {
				return handle == IntPtr.Zero || handle == new IntPtr (-1);
			}
		}

		public CacheKey GetCacheKey()
		{
			return new CacheKey.Builder ().AddComponent (handle).Build ();
		}
	}
}

