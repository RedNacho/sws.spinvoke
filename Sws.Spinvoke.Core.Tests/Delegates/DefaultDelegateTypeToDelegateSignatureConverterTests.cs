using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;

using Sws.Spinvoke.Core.Delegates;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DefaultDelegateTypeToDelegateSignatureConverterTests
	{
		[Test ()]
		public void ReturnsCorrectSignatureForAction()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter();

			var delegateSignature = subject.CreateDelegateSignature (typeof(Action<int, string>), CallingConvention.Cdecl);

			Assert.IsTrue (delegateSignature.InputTypes.SequenceEqual (new [] { typeof(int), typeof(string) }));
			Assert.AreEqual (typeof(void), delegateSignature.OutputType);
			Assert.AreEqual (CallingConvention.Cdecl, delegateSignature.CallingConvention);
		}

		[Test ()]
		public void ReturnsCorrectSignatureForFunc()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter();

			var delegateSignature = subject.CreateDelegateSignature (typeof(Func<int, string, double>), CallingConvention.Cdecl);

			Assert.IsTrue (delegateSignature.InputTypes.SequenceEqual (new [] { typeof(int), typeof(string) }));
			Assert.AreEqual (typeof(double), delegateSignature.OutputType);
			Assert.AreEqual (CallingConvention.Cdecl, delegateSignature.CallingConvention);
		}
	}
}

