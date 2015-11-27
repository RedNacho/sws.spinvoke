using System;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core;
using Sws.Spinvoke.Core.Exceptions;

using Sws.Spinvoke.Core.LibraryLoading;

namespace Sws.Spinvoke.Linux
{
	public class LinuxNativeLibraryLoader : LibDlNativeLibraryLoader<LinuxLibDlManaged.LinuxSafeLibraryHandle>
	{
		public LinuxNativeLibraryLoader() : base(new LinuxLibDlManaged())
		{
		}
	}
}

