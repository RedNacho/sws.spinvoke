using System;

namespace Sws.Spinvoke.Interception
{
	public enum PointerManagementMode
	{
		DestroyAfterCall,
		DestroyOnInterceptionGarbageCollect,
		DoNotDestroy
	}
}

