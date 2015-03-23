using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;

using Moq;

using Sws.Spinvoke.Core.Delegates;
using Sws.Spinvoke.Core.Delegates.Generics;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DefaultGenericDelegateTypeConverterTests
	{
		public delegate void TestDelegate();

		[Test ()]
		public void ConvertsDelegateTypeToInteropSupportedDelegate ()
		{
			var delegateTypeProviderMock = new Mock<IDelegateTypeProvider>();

			var delegateTypeToDelegateSignatureConverterMock = new Mock<IDelegateTypeToDelegateSignatureConverter> ();

			var delegateSignature = new DelegateSignature (new Type[0], typeof(object), CallingConvention.Cdecl);

			delegateTypeToDelegateSignatureConverterMock.Setup (dt => dt.CreateDelegateSignature (It.IsAny<Type> (), It.IsAny<CallingConvention> ()))
				.Returns (delegateSignature);

			delegateTypeProviderMock.Setup(dtp => dtp.GetDelegateType(It.IsAny<DelegateSignature>()))
				.Returns(typeof(TestDelegate));

			var subject = new DefaultGenericDelegateTypeConverter(delegateTypeToDelegateSignatureConverterMock.Object, delegateTypeProviderMock.Object);

			var convertedDelegateType = subject.ConvertToInteropSupportedDelegateType (typeof(Action<int, string>), CallingConvention.Cdecl);

			Assert.AreEqual (typeof(TestDelegate), convertedDelegateType);

			delegateTypeToDelegateSignatureConverterMock.Verify(dttds => dttds.CreateDelegateSignature(typeof(Action<int, string>), CallingConvention.Cdecl), Times.Once);

			delegateTypeToDelegateSignatureConverterMock.Verify(dttds => dttds.CreateDelegateSignature(It.IsAny<Type>(), It.IsAny<CallingConvention>()), Times.Once);

			delegateTypeProviderMock.Verify(dtp => dtp.GetDelegateType(delegateSignature), Times.Once);

			delegateTypeProviderMock.Verify (dtp => dtp.GetDelegateType (It.IsAny<DelegateSignature> ()), Times.Once);
		}
	}
}


