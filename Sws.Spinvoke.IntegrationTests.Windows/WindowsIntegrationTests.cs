using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

using Sws.Spinvoke.Core;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.IntegrationTests.Agnostic;
using Sws.Spinvoke.Interception.ArgumentPreprocessing;
using Sws.Spinvoke.Interception.MemoryManagement;
using Sws.Spinvoke.Interception.ReturnPostprocessing;
using Sws.Spinvoke.Windows;

namespace Sws.Spinvoke.IntegrationTests.Windows
{
	[TestFixture ()]
	public class WindowsIntegrationTests : PlatformIntegrationTestsBase
	{
		protected override INativeLibraryLoader CreateNativeLibraryLoader ()
		{
			return new WindowsNativeLibraryLoader ();
		}

		protected override string LibraryName
		{
			get { return "libsws.spinvoke.windows.dll"; }
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
		[return: NativeReturnsGccStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		int Add([NativeArgumentAsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] int x, [NativeArgumentAsStructPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] int y);
	}

	public interface IDynamicProxyStringTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsGccAnsiStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		string ReverseString([NativeArgumentAsAnsiStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)] string input);
	}

	public interface IDynamicProxyManualMemoryReleaseTest
	{
		[NativeDelegateDefinitionOverride(FunctionName = "reverseString")]
		[return: NativeReturnsGccAnsiStringPointer(pointerManagementMode: PointerManagementMode.DestroyAfterCall)]
		string ReverseString([NativeArgumentAsAnsiStringPointer(pointerManagementMode: PointerManagementMode.DestroyOnInterceptionGarbageCollect)] string input);
	}

    public class NativeReturnsGccStructPointerAttribute : NativeReturnDefinitionOverrideAttribute
    {
        public NativeReturnsGccStructPointerAttribute(PointerManagementMode pointerManagementMode = PointerManagementMode.DoNotDestroy)
            : base(new PointerToStructReturnPostprocessor(pointerManagementMode, new GccPointerMemoryManager()), typeof(IntPtr))
        {
        }
    }

    public class NativeReturnsGccAnsiStringPointerAttribute : NativeReturnDefinitionOverrideAttribute
    {
        public NativeReturnsGccAnsiStringPointerAttribute(PointerManagementMode pointerManagementMode = PointerManagementMode.DoNotDestroy)
            : base(new PointerToAnsiStringReturnPostprocessor(pointerManagementMode, new GccPointerMemoryManager()), typeof(IntPtr))
        {
        }
    }

    public class NativeArgumentAsAnsiStringPointerAttribute : NativeArgumentDefinitionOverrideAttribute
    {
        public NativeArgumentAsAnsiStringPointerAttribute(PointerManagementMode pointerManagementMode = PointerManagementMode.DestroyAfterCall)
            : base(new AnsiStringToPointerArgumentPreprocessor(pointerManagementMode, DefaultPointerMemoryManager), typeof(IntPtr))
        {
        }
    }

    public class PointerToAnsiStringReturnPostprocessor : PointerToStringReturnPostprocessor
    {
        public PointerToAnsiStringReturnPostprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager) : base(pointerManagementMode, pointerMemoryManager)
        {
        }

        protected override string PtrToString(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }

    public class AnsiStringToPointerArgumentPreprocessor : StringToPointerArgumentPreprocessor
    {
        public AnsiStringToPointerArgumentPreprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager) : base(pointerManagementMode, pointerMemoryManager)
        {
        }

        protected override IntPtr StringToPointer(string input)
        {
            return Marshal.StringToHGlobalAnsi(input);
        }
    }

    public class GccPointerMemoryManager : PointerMemoryManager
    {
        [DllImport("msvcrt.dll")]
        private static extern void free(IntPtr ptr);

        protected override void DefaultFreeAction(IntPtr ptr)
        {
            free(ptr);
        }
    }
}