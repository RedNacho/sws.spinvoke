using System;
using Sws.Spinvoke.Core.LibraryLoading;

namespace Sws.Spinvoke.OSX
{
	public class OSXNativeLibraryLoader : LibDlNativeLibraryLoader<OSXLibDlManaged.OSXSafeLibraryHandle>
	{
		public OSXNativeLibraryLoader() : base(new OSXLibDlManaged())
		{
		}
	}
}

