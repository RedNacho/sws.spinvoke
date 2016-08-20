using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Sws.Spinvoke.Bootstrapper
{
	/// <summary>
	/// OS detector. Ripped off from here: https://github.com/jpobst/Pinta/blob/master/Pinta.Core/Managers/SystemManager.cs
	/// 
	/// Author:
	///       Jonathan Pobst <monkey@jpobst.com>
	/// 
	/// Copyright (c) 2010 Jonathan Pobst
	/// 
	/// Permission is hereby granted, free of charge, to any person obtaining a copy
	/// of this software and associated documentation files (the "Software"), to deal
	/// in the Software without restriction, including without limitation the rights
	/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	/// copies of the Software, and to permit persons to whom the Software is
	/// furnished to do so, subject to the following conditions:
	/// 
	/// The above copyright notice and this permission notice shall be included in
	/// all copies or substantial portions of the Software.
	/// 
	/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	/// THE SOFTWARE.
	/// </summary>
	public static class OSDetector
	{
		public static OS DetectOS() {
			if (Path.DirectorySeparatorChar == '\\')
				return OS.Windows;
			else if (IsRunningOnMac ())
				return OS.Mac;
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
				return OS.X11;
			else
				return OS.Other;
		}

		//From Managed.Windows.Forms/XplatUI
		[DllImport ("libc")]
		private static extern int uname (IntPtr buf);

		private static bool IsRunningOnMac ()
		{
			IntPtr buf = IntPtr.Zero;
			try {
				buf = Marshal.AllocHGlobal (8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname (buf) == 0) {
					string os = Marshal.PtrToStringAnsi (buf);
					if (os == "Darwin")
						return true;
				}
			} catch {
			} finally {
				if (buf != IntPtr.Zero)
					Marshal.FreeHGlobal (buf);
			}
			return false;
		}
	}
}