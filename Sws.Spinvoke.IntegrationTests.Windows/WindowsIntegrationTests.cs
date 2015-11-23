using NUnit.Framework;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Linux;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.IntegrationTests.Agnostic;

namespace Sws.Spinvoke.IntegrationTests.Windows
{
	[TestFixture ()]
	public class WindowsIntegrationTests : PlatformIntegrationTestsBase
	{
		protected override INativeLibraryLoader CreateNativeLibraryLoader ()
		{
			return new LinuxNativeLibraryLoader ();
		}

		protected override string LibraryName
		{
			get { return "libSws.Spinvoke.IntegrationTests.so"; }
		}

		[Test ()]
		public void NativeCodeInvokedWithStructPointerConversion()
		{
			NativeCodeInvokedWithStructPointerConversion (
				(IDynamicProxyPointerTest proxy, int x, int y) => proxy.Add(x, y)
			);
		}

		[Test ()]
		public void NativeCodeInvokedWithStringPointerConversion()
		{
			NativeCodeInvokedWithStringPointerConversion (
				(IDynamicProxyStringTest proxy, string input) => proxy.ReverseString(input)
			);
		}

		[Test ()]
		public void InterceptionAllocatedMemoryManagerAllowsManualDeallocationOfGeneratedPointers()
		{
			InterceptionAllocatedMemoryManagerAllowsManualDeallocationOfGeneratedPointers (
				(IDynamicProxyManualMemoryReleaseTest proxy, string input) => proxy.ReverseString(input)
			);
		}
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



