using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Sws.Spinvoke.Interception.MemoryManagement;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class PointerMemoryManagerTests
	{
		public class TestPointerMemoryManager : PointerMemoryManager
		{
			private readonly Action<IntPtr> _defaultFreeAction;

			public TestPointerMemoryManager () : this(null)
			{
			}

			public TestPointerMemoryManager (Action<IntPtr> defaultFreeAction)
			{
				if (defaultFreeAction == null) {
					defaultFreeAction = ptr => { };
				}

				_defaultFreeAction = defaultFreeAction;
			}

			protected override void DefaultFreeAction (IntPtr ptr)
			{
				_defaultFreeAction (ptr);
			}
		}

		[Test ()]
		public void ReportPointerCallCompletedWithDestroyAfterCallDisposesPointer()
		{
			var subject = new TestPointerMemoryManager ();

			var intPtr = new IntPtr (12345);

			var disposed = new List<IntPtr> ();

			subject.ReportPointerCallCompleted (intPtr, PointerManagementMode.DestroyAfterCall, disposed.Add);

			Assert.AreEqual (1, disposed.Count);

			Assert.AreEqual (intPtr, disposed.Single ());
		}

		[Test ()]
		public void ReportPointerCallCompletedWithDestroyAfterCallFallsBackToDefaultFreeAction ()
		{
			var disposed = new List<IntPtr> ();

			var subject = new TestPointerMemoryManager (disposed.Add);

			var intPtr = new IntPtr (12345);

			subject.ReportPointerCallCompleted (intPtr, PointerManagementMode.DestroyAfterCall);

			Assert.AreEqual (1, disposed.Count);

			Assert.AreEqual (intPtr, disposed.Single ());
		}

		[Test ()]
		public void ReportPointerCallCompletedWithDoNotDestroyDoesNotDisposeMemory()
		{
			var subject = new TestPointerMemoryManager ();

			var intPtr = new IntPtr (12345);

			var disposed = new List<IntPtr> ();

			subject.ReportPointerCallCompleted (intPtr, PointerManagementMode.DoNotDestroy, disposed.Add);

			subject.GarbageCollectAll ();

			Assert.AreEqual (0, disposed.Count);
		}

		[Test ()]
		public void ReportPointerCallCompletedWithDoNotDestroyDoesNotFallBackToDefaultFreeAction ()
		{
			var disposed = new List<IntPtr> ();

			var subject = new TestPointerMemoryManager (disposed.Add);

			var intPtr = new IntPtr (12345);

			subject.ReportPointerCallCompleted (intPtr, PointerManagementMode.DoNotDestroy);

			subject.GarbageCollectAll ();

			Assert.AreEqual (0, disposed.Count);
		}

		[Test ()]
		public void ReportPointerCallCompletedWithDestroyOnInterceptionGarbageCollectDisposesMemoryOnGarbageCollectionCall()
		{
			var subject = new TestPointerMemoryManager ();

			var intPtr = new IntPtr (12345);

			var disposed = new List<IntPtr> ();

			subject.ReportPointerCallCompleted (intPtr, PointerManagementMode.DestroyOnInterceptionGarbageCollect, disposed.Add);

			var disposedBeforeGarbageCollection = disposed.Count;

			subject.GarbageCollectAll ();

			Assert.AreEqual (0, disposedBeforeGarbageCollection);

			Assert.AreEqual (1, disposed.Count);
		}

		[Test ()]
		public void ReportPointerCallCompletedWithDestroyOnInterceptionGarbageCollectFallsBackToDefaultFreeAction ()
		{
			var disposed = new List<IntPtr> ();

			var subject = new TestPointerMemoryManager (disposed.Add);

			var intPtr = new IntPtr (12345);

			subject.ReportPointerCallCompleted (intPtr, PointerManagementMode.DestroyOnInterceptionGarbageCollect);

			var disposedBeforeGarbageCollection = disposed.Count;

			subject.GarbageCollectAll ();

			Assert.AreEqual (0, disposedBeforeGarbageCollection);

			Assert.AreEqual (1, disposed.Count);
		}

		[Test ()]
		public void RegisterForGarbageCollectionDisposesMemoryOnGarbageCollectionCall()
		{
			var subject = new TestPointerMemoryManager ();

			var intPtr = new IntPtr (12345);

			var disposed = new List<IntPtr> ();

			subject.RegisterForGarbageCollection (intPtr, disposed.Add);

			var disposedBeforeGarbageCollection = disposed.Count;

			subject.GarbageCollectAll ();

			Assert.AreEqual (0, disposedBeforeGarbageCollection);

			Assert.AreEqual (1, disposed.Count);
		}

		[Test ()]
		public void RegisterForGarbageCollectionFallsBackOnDefaultFreeAction ()
		{
			var disposed = new List<IntPtr> ();

			var subject = new TestPointerMemoryManager (disposed.Add);

			var intPtr = new IntPtr (12345);

			subject.RegisterForGarbageCollection (intPtr);

			var disposedBeforeGarbageCollection = disposed.Count;

			subject.GarbageCollectAll ();

			Assert.AreEqual (0, disposedBeforeGarbageCollection);

			Assert.AreEqual (1, disposed.Count);
		}

		[Test ()]
		public void GarbageCollectAllThrowsSingleExceptionAfterAllProcessing()
		{
			var subject = new TestPointerMemoryManager ();

			var intPtr = new IntPtr (12345);

			var intPtr2 = new IntPtr (67890);

			var testException = new Exception ();

			var disposed = new List<IntPtr> ();

			subject.RegisterForGarbageCollection (intPtr, ptr => { throw testException; });

			subject.RegisterForGarbageCollection (intPtr2, disposed.Add);

			Exception thrownException = null;

			try
			{
				subject.GarbageCollectAll();
			}
			catch (Exception ex) {
				thrownException = ex;
			}

			Assert.AreEqual (testException, thrownException);

			Assert.AreEqual (1, disposed.Count);

			Assert.AreEqual (intPtr2, disposed.Single ());
		}

		[Test ()]
		public void GarbageCollectAllThrowsAggregateExceptionForMultipleExceptions ()
		{
			var subject = new TestPointerMemoryManager ();

			var intPtr = new IntPtr (12345);

			var intPtr2 = new IntPtr (67890);

			var testException = new Exception ();

			var testException2 = new Exception ();

			subject.RegisterForGarbageCollection (intPtr, ptr => { throw testException; });

			subject.RegisterForGarbageCollection (intPtr2, ptr => { throw testException2; });

			AggregateException thrownException = null;

			try
			{
				subject.GarbageCollectAll();
			}
			catch (Exception ex) {
				thrownException = ex as AggregateException;
			}

			Assert.IsNotNull (thrownException);

			Assert.AreEqual (2, thrownException.InnerExceptions.Count);

			Assert.IsTrue (thrownException.InnerExceptions.Any(ex => ex == testException));
			Assert.IsTrue (thrownException.InnerExceptions.Any(ex => ex == testException2));
		}

		[Test ()]
		public void HasGarbageCollectibleMemoryReturnsTrueIfGarbageCollectibleMemory ()
		{
			var subject = new TestPointerMemoryManager ();

			var garbageCollectibleMemoryAtStart = subject.HasGarbageCollectibleMemory ();

			subject.RegisterForGarbageCollection (new IntPtr(1), ptr => { });
			subject.RegisterForGarbageCollection (new IntPtr(2), ptr => { });

			var garbageCollectibleMemoryAfterQueueing = subject.HasGarbageCollectibleMemory ();

			subject.GarbageCollectAll ();

			var garbageCollectibleMemoryAfterGarbageCollect = subject.HasGarbageCollectibleMemory ();

			subject.RegisterForGarbageCollection (new IntPtr (3), ptr => {
				throw new Exception ();
			});

			Exception thrownException = null;

			try
			{
				subject.GarbageCollectAll ();
			}
			catch (Exception ex) {
				thrownException = ex;
			}

			var garbageCollectibleMemoryAfterFailedGarbageCollect = subject.HasGarbageCollectibleMemory ();

			Assert.IsFalse (garbageCollectibleMemoryAtStart);
			Assert.IsTrue (garbageCollectibleMemoryAfterQueueing);
			Assert.IsFalse (garbageCollectibleMemoryAfterGarbageCollect);
			Assert.IsNotNull (thrownException);
			Assert.IsTrue (garbageCollectibleMemoryAfterFailedGarbageCollect);
		}

		[Test ()]
		public void GarbageCollectNamedDisposesOfNamedBlock()
		{
			var subject = new TestPointerMemoryManager ();

			var disposed = new List<IntPtr> ();

			subject.RegisterForGarbageCollection (new IntPtr (1), disposed.Add);

			subject.BeginNamedBlock ("Test");

			subject.RegisterForGarbageCollection (new IntPtr (2), disposed.Add);
			subject.RegisterForGarbageCollection (new IntPtr (3), disposed.Add);

			subject.EndNamedBlock ();
		
			subject.RegisterForGarbageCollection (new IntPtr (4), disposed.Add);

			subject.GarbageCollectNamed ("Test");

			Assert.AreEqual (2, disposed.Count);

			Assert.IsTrue(disposed.Any (ptr => ptr == new IntPtr(2)));
			Assert.IsTrue(disposed.Any (ptr => ptr == new IntPtr(3)));

			Assert.IsTrue (subject.HasGarbageCollectibleMemory ());
		}

		[Test ()]
		public void GarbageCollectAllIncludesNamedBlock()
		{
			var subject = new TestPointerMemoryManager ();

			var disposed = new List<IntPtr> ();

			subject.RegisterForGarbageCollection (new IntPtr (1), disposed.Add);

			subject.BeginNamedBlock ("Test");

			subject.RegisterForGarbageCollection (new IntPtr (2), disposed.Add);
			subject.RegisterForGarbageCollection (new IntPtr (3), disposed.Add);

			subject.EndNamedBlock ();

			subject.RegisterForGarbageCollection (new IntPtr (4), disposed.Add);

			subject.GarbageCollectAll ();

			Assert.AreEqual (4, disposed.Count);

			Assert.IsTrue(disposed.Any (ptr => ptr == new IntPtr(1)));
			Assert.IsTrue(disposed.Any (ptr => ptr == new IntPtr(2)));
			Assert.IsTrue(disposed.Any (ptr => ptr == new IntPtr(3)));
			Assert.IsTrue(disposed.Any (ptr => ptr == new IntPtr(4)));

			Assert.IsFalse (subject.HasGarbageCollectibleMemory ());
		}

		[Test ()]
		public void HasGarbageCollectibleMemoryIncludesNamedBlock()
		{
			var subject = new TestPointerMemoryManager ();

			subject.BeginNamedBlock ("Test");

			subject.RegisterForGarbageCollection (new IntPtr (1), ptr => { });

			subject.EndNamedBlock ();

			Assert.IsTrue (subject.HasGarbageCollectibleMemory ());
		}

		[Test ()]
		public void GetNamedBlocksWithGarbageCollectibleMemoryReturnsNamedBlocksWithPointersQueued()
		{
			var subject = new TestPointerMemoryManager ();

			subject.BeginNamedBlock ("Test1");

			subject.RegisterForGarbageCollection (new IntPtr (1), ptr => { });

			subject.EndNamedBlock ();

			subject.BeginNamedBlock ("Test2");

			subject.RegisterForGarbageCollection (new IntPtr (2), ptr => { });

			subject.EndNamedBlock ();

			subject.BeginNamedBlock ("Test3");

			subject.RegisterForGarbageCollection (new IntPtr (3), ptr => { });

			subject.EndNamedBlock ();

			subject.RegisterForGarbageCollection (new IntPtr (4), ptr => { });

			subject.GarbageCollectNamed ("Test3");

			var blocks = subject.GetNamedBlocksWithGarbageCollectibleMemory ().ToArray ();

			Assert.AreEqual (2, blocks.Count());
			Assert.IsTrue (blocks.Any (block => block == "Test1"));
			Assert.IsTrue (blocks.Any (block => block == "Test2"));
		}

		[Test ()]
		public void HasUnnamedGarbageCollectibleMemoryReturnsTrueIfUnnamedGarbageCollectibleMemory ()
		{
			var subject = new TestPointerMemoryManager ();

			subject.BeginNamedBlock ("Test");

			subject.RegisterForGarbageCollection (new IntPtr (0), ptr => { });

			subject.EndNamedBlock ();

			var unnamedBeforeQueueing = subject.HasUnnamedGarbageCollectibleMemory ();

			subject.RegisterForGarbageCollection (new IntPtr (1), ptr => { });

			Assert.IsFalse (unnamedBeforeQueueing);

			Assert.IsTrue (subject.HasUnnamedGarbageCollectibleMemory ());
		}

		[Test ()]
		public void GarbageCollectCurrentBlockCollectsCurrentNamedBlock ()
		{
			var subject = new TestPointerMemoryManager ();

			var disposed = new List<IntPtr> ();

			subject.BeginNamedBlock ("Test");

			subject.RegisterForGarbageCollection (new IntPtr (1), disposed.Add);

			subject.EndNamedBlock ();

			subject.BeginNamedBlock ("Test2");

			subject.RegisterForGarbageCollection (new IntPtr (2), disposed.Add);

			subject.GarbageCollectCurrentBlock ();

			subject.EndNamedBlock ();

			Assert.AreEqual (1, disposed.Count);
			Assert.AreEqual (new IntPtr (2), disposed.Single ());
		}
	}
}


