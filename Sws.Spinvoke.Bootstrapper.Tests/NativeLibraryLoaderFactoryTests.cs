using NUnit.Framework;
using System;

using Sws.Spinvoke.Bootstrapper;
using Sws.Spinvoke.Linux;
using Sws.Spinvoke.Windows;
using Sws.Spinvoke.OSX;

namespace Sws.Spinvoke.Bootstrapper.Tests
{
	[TestFixture ()]
	public class NativeLibraryLoaderFactoryTests
	{
		[Test ()]
		public void ReturnsLinuxNativeLibraryLoaderForX11 ()
		{
			var loader = (new NativeLibraryLoaderFactory (() => OS.X11)).Create ();

			Assert.AreEqual (typeof(LinuxNativeLibraryLoader), loader.GetType());
		}

		[Test ()]
		public void ReturnsWindowsNativeLibraryLoaderForWindows ()
		{
			var loader = (new NativeLibraryLoaderFactory (() => OS.Windows)).Create ();

			Assert.AreEqual (typeof(WindowsNativeLibraryLoader), loader.GetType());
		}

		[Test ()]
		public void ReturnsOSXNativeLibraryLoaderForMac ()
		{
			var loader = (new NativeLibraryLoaderFactory (() => OS.Mac)).Create ();

			Assert.AreEqual (typeof(OSXNativeLibraryLoader), loader.GetType());
		}

		[Test ()]
		public void ThrowsExceptionForUnrecognisedOS ()
		{
			Assert.Throws<InvalidOperationException> (() => 
				(new NativeLibraryLoaderFactory (() => OS.Other)).Create ());
		}
	}
}

