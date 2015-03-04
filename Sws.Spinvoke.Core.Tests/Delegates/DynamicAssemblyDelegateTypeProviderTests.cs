using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Sws.Spinvoke.Core.Delegates;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DynamicAssemblyDelegateProviderTests
	{
		[Test ()]
		public void GetDelegateTypeCreatesWorkingDelegate()
		{
			var provider = new DynamicAssemblyDelegateTypeProvider ("TestAssembly");

			var delegateType = provider.GetDelegateType (new DelegateSignature(new[] { typeof(int), typeof(int) }, typeof(int), CallingConvention.Cdecl));

			var instance = Delegate.CreateDelegate (delegateType, GetAddMethodInfo ());

			var result = instance.DynamicInvoke (new object[] { 2, 3 });

			Assert.AreEqual (5, result);
		}

		[Test ()]
		public void GetDelegateTypeReturnsDifferentInstanceEachCall()
		{
			var provider = new DynamicAssemblyDelegateTypeProvider ("TestAssembly");

			var inputTypes = new [] { typeof(int), typeof(int) };
			var outputType = typeof(int);

			var callingConvention = CallingConvention.Cdecl;

			var delegateType1 = provider.GetDelegateType (new DelegateSignature(inputTypes, outputType, callingConvention));
			var delegateType2 = provider.GetDelegateType (new DelegateSignature(inputTypes, outputType, callingConvention));

			Assert.IsNotNull (delegateType1);
			Assert.IsNotNull (delegateType2);
			Assert.AreNotEqual (delegateType1, delegateType2);
		}

		private MethodInfo GetAddMethodInfo()
		{
			return typeof(DynamicAssemblyDelegateProviderTests).GetMethod ("Add", BindingFlags.NonPublic | BindingFlags.Static);
		}
	
		private static int Add(int x, int y)
		{
			return x + y;
		}
	}
}

