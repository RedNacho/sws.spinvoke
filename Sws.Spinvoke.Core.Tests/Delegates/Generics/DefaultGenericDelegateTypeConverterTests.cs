using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Moq;

using Sws.Spinvoke.Core.Delegates.Generics;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DefaultGenericDelegateTypeConverterTests
	{
		public delegate void TestDelegate();

		[Test ()]
		public void ConvertsActionToDelegate ()
		{
			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider>();

			delegateTypeProviderMock.Setup(dtp => dtp.GetDelegateType(It.IsAny<DelegateSignature>()))
				.Returns(typeof(TestDelegate));

			var subject = new DefaultGenericDelegateTypeConverter(delegateTypeProviderMock.Object);

			var convertedDelegateType = subject.ConvertToInteropSupportedDelegateType (typeof(Action<int, string>), CallingConvention.Cdecl);

			Assert.AreEqual (typeof(TestDelegate), convertedDelegateType);

			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(It.Is<DelegateSignature>(ds => ds.CallingConvention == CallingConvention.Cdecl)), Times.Once);
			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(It.Is<DelegateSignature>(ds => ds.InputTypes.SequenceEqual(new [] { typeof(int), typeof(string) }))), Times.Once);
			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(It.Is<DelegateSignature>(ds => ds.OutputType == typeof(void))), Times.Once);

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()), Times.Once);
		}

		[Test ()]
		public void ConvertsFuncToDelegate ()
		{
			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider>();

			delegateTypeProviderMock.Setup(dtp => dtp.GetDelegateType(It.IsAny<DelegateSignature>()))
				.Returns(typeof(TestDelegate));

			var subject = new DefaultGenericDelegateTypeConverter(delegateTypeProviderMock.Object);

			var convertedDelegateType = subject.ConvertToInteropSupportedDelegateType (typeof(Func<int, string, double>), CallingConvention.Cdecl);

			Assert.AreEqual (typeof(TestDelegate), convertedDelegateType);

			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(It.Is<DelegateSignature>(ds => ds.CallingConvention == CallingConvention.Cdecl)), Times.Once);
			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(It.Is<DelegateSignature>(ds => ds.InputTypes.SequenceEqual(new [] { typeof(int), typeof(string) }))), Times.Once);
			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(It.Is<DelegateSignature>(ds => ds.OutputType == typeof(double))), Times.Once);

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()), Times.Once);
		}
	}
}


