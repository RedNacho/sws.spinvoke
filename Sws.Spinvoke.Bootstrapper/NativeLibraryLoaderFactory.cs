using System;
using Sws.Spinvoke.Core;
using Sws.Spinvoke.Linux;
using Sws.Spinvoke.Windows;
using Sws.Spinvoke.OSX;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Bootstrapper
{
	public class NativeLibraryLoaderFactory
	{
		private readonly Func<OS> _osDetector;

		public NativeLibraryLoaderFactory (Func<OS> osDetector)
		{
			if (osDetector == null) {
				throw new ArgumentNullException ("osDetector");
			}

			this._osDetector = osDetector;
		}

		public INativeLibraryLoader Create() {
			var os = _osDetector ();

			switch (os) {
			case OS.X11:
				return new LinuxNativeLibraryLoader ();
			case OS.Windows:
				return new WindowsNativeLibraryLoader ();
			case OS.Mac:
				return new OSXNativeLibraryLoader ();
			default:
				throw new InvalidOperationException (string.Format("Detected OS type ({0}) not supported. You may need to implement your own INativeLibraryLoader.", os));
			}
		}
	}
}

