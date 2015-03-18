﻿using System;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception
{
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class NativeReturnsStringPointerAttribute : NativeReturnDefinitionOverrideAttribute
	{
		public NativeReturnsStringPointerAttribute(PointerManagementMode pointerManagementMode = PointerManagementMode.DestroyAfterCall)
			: base(new PointerToStringReturnPostprocessor(pointerManagementMode), typeof(IntPtr))
		{
		}
	}
}

