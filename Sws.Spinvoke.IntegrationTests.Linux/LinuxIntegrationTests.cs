using System.Runtime.InteropServices;
using NUnit.Framework;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Linux;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.IntegrationTests.Agnostic;
using Sws.Spinvoke.Bootstrapper;

namespace Sws.Spinvoke.IntegrationTests.Linux
{
	[TestFixture ()]
	public class LinuxIntegrationTests : PlatformIntegrationTestsBase
	{
		[TestFixtureSetUp()]
		public void TestFixtureSetUp() {
			if (OSDetector.DetectOS () != OS.X11) {
				Assert.Ignore ("Non-Linux OS detected. If this is a mistake, please remove this clause.");
			}
		}

		protected override INativeLibraryLoader CreateNativeLibraryLoader ()
		{
			return new LinuxNativeLibraryLoader ();
		}

		protected override string LibraryName
		{
			get { return "libSws.Spinvoke.IntegrationTests.so"; }
		}

		protected override CallingConvention CallingConvention
		{
			get { return CallingConvention.Winapi; }
		}

		[Test()]
		public void NativeCodeInvokedThroughExplicitDelegateTypeIfSpecified()
		{
			TestNativeAddFunctionWithDecimalResult(
				(IDynamicProxyExplicitDelegateTypeTest proxy, int x, int y) => proxy.Add(x, y)
			);
		}

		[Test ()]
		public void NativeCodeInvokedWithStructPointerConversion()
		{
			TestNativeAddFunction (
				(IDynamicProxyPointerTest proxy, int x, int y) => proxy.Add(x, y)
			);
		}

		[Test ()]
		public void NativeCodeInvokedWithStringPointerConversion()
		{
			TestNativeReverseStringFunction (
				(IDynamicProxyStringTest proxy, string input) => proxy.ReverseString(input)
			);
		}

		[Test ()]
		public void InterceptionAllocatedMemoryManagerAllowsManualDeallocationOfGeneratedPointers()
		{
			TestNativeReverseStringFunctionWithManualReleaseInput (
				(IDynamicProxyManualMemoryReleaseTest proxy, string input) => proxy.ReverseString(input)
			);
		}
	}

	public delegate int ExplicitAddDelegate(int x, int y);

	public interface IDynamicProxyExplicitDelegateTypeTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "add", ExplicitDelegateType = typeof(ExplicitAddDelegate))]
		decimal Add(int x, int y);
	}

	public interface IDynamicProxyPointerTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "pointerAdd")]
		[return: NativeReturnsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		int Add([NativeArgumentAsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] int x, [NativeArgumentAsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] int y);
	}

	public interface IDynamicProxyStringTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		string ReverseString([NativeArgumentAsStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] string input);
	}

	public interface IDynamicProxyManualMemoryReleaseTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		string ReverseString([NativeArgumentAsStringPointer(pointerManagementMode: PointerManagementMode.DestroyOnInterceptionGarbageCollect)] string input);
	}
}

