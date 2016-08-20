using NUnit.Framework;
using System;

namespace Sws.Spinvoke.Bootstrapper.Tests
{
	[TestFixture ()]
	public class BootstrapperTests
	{
		[Test ()]
		public void BootstrapperCanCreateCoreFacadeForYourOS ()
		{
			Bootstrapper.CreateCoreFacadeForOS ();
		}
	}
}

