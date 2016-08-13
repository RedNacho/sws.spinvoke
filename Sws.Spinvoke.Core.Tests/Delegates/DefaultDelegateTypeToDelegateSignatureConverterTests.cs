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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void UnmanagedFunction();

		[Test ()]
		public void ReturnsCorrectSignatureForActionWithCallingConventionSupplied()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter();

			var delegateSignature = subject.CreateDelegateSignature (typeof(Action<int, string>), CallingConvention.Cdecl);

			Assert.IsTrue (delegateSignature.InputTypes.SequenceEqual (new [] { typeof(int), typeof(string) }));
			Assert.AreEqual (typeof(void), delegateSignature.OutputType);
			Assert.AreEqual (CallingConvention.Cdecl, delegateSignature.CallingConvention);
		}

		[Test ()]
		public void ReturnsCorrectSignatureForUnmanagedFunctionWithoutRequiringCallingConvention()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter();

			var delegateSignature = subject.CreateDelegateSignature (typeof(UnmanagedFunction));

			Assert.IsTrue (delegateSignature.InputTypes.SequenceEqual (new Type[0]));
			Assert.AreEqual (typeof(void), delegateSignature.OutputType);
			Assert.AreEqual (CallingConvention.Cdecl, delegateSignature.CallingConvention);
		}

		[Test ()]
		public void ThrowsInvalidOperationExceptionIfCallingConventionNotSupplied()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter();

			Assert.Throws<InvalidOperationException>(() => subject.CreateDelegateSignature (typeof(Action<int, string>)));
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

		[Test ()]
		public void HasCallingConventionReturnsFalseForFunc()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter();

			var hasCallingConvention = subject.HasCallingConvention(typeof(Func<int, bool>));

			Assert.IsFalse(hasCallingConvention);
		}

		[Test ()]
		public void HasCallingConventionReturnsTrueForDelegateWithCallingConvention()
		{
			var subject = new DefaultDelegateTypeToDelegateSignatureConverter ();

			var hasCallingConvention = subject.HasCallingConvention (typeof(UnmanagedFunction));

			Assert.IsTrue (hasCallingConvention);
		}
	}
}

