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

	    protected override CallingConvention CallingConvention
	    {
	        get { return CallingConvention.Cdecl; }
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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int ExplicitAddDelegate(int x, int y);

    public interface IDynamicProxyExplicitDelegateTypeTest
    {
        [NativeDelegateDefinitionOverride(FunctionName = "add", ExplicitDelegateType = typeof(ExplicitAddDelegate))]
        decimal Add(int x, int y);
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
            : base(new GccPointerToStructReturnPostprocessor(pointerManagementMode, DefaultPointerMemoryManager), typeof(IntPtr))
        {
        }
    }

    public class NativeReturnsGccAnsiStringPointerAttribute : NativeReturnDefinitionOverrideAttribute
    {
        public NativeReturnsGccAnsiStringPointerAttribute(PointerManagementMode pointerManagementMode = PointerManagementMode.DoNotDestroy)
            : base(new GccPointerToAnsiStringReturnPostprocessor(pointerManagementMode, DefaultPointerMemoryManager), typeof(IntPtr))
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

    public class GccPointerToStructReturnPostprocessor : PointerToStructReturnPostprocessor
    {
        public GccPointerToStructReturnPostprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager) : base(pointerManagementMode, pointerMemoryManager)
        {
        }

        protected override bool IsFreePointerImplemented
        {
            get { return true; }
        }

        protected override void FreePointer(IntPtr pointer)
        {
            GccHelper.FreePointer(pointer);
        }
    }

    public class GccPointerToAnsiStringReturnPostprocessor : PointerToStringReturnPostprocessor
    {
        public GccPointerToAnsiStringReturnPostprocessor(PointerManagementMode pointerManagementMode, PointerMemoryManager pointerMemoryManager) : base(pointerManagementMode, pointerMemoryManager)
        {
        }

        protected override string PtrToString(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }

        protected override bool IsFreePointerImplemented
        {
            get { return true; }
        }
        
        protected override void FreePointer(IntPtr pointer)
        {
            GccHelper.FreePointer(pointer);
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

    public class GccHelper
    {
        private GccHelper() { }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void free(IntPtr ptr);

        public static void FreePointer(IntPtr ptr)
        {
            free(ptr);
        }
    }
}