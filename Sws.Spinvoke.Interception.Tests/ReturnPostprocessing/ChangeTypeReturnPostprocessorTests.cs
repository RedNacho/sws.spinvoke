using NUnit.Framework;
using System;
using System.Linq;

using Sws.Spinvoke.Interception.ReturnPostprocessing;

namespace Sws.Spinvoke.Interception.Tests
{
	[TestFixture ()]
	public class ChangeTypeReturnPostprocessorTests
	{
		[Test ()]
		public void CanProcessReturnsTrueForAllTypes ()
		{
			var conversionPairs = new Tuple<Type, object>[] {
				Tuple.Create<Type, object>(typeof(object), 45),
				Tuple.Create<Type, object>(typeof(int), new Object()),
				Tuple.Create<Type, object>(typeof(object), new Object()),
				Tuple.Create<Type, object>(typeof(int), 45)
			};

			var canProcess = conversionPairs.Select (pair => new ChangeTypeReturnPostprocessor ().CanProcess (pair.Item2, pair.Item1)).All (result => result);

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void WhenRequiredTypeIsStringReturnsString()
		{
			var subject = new ChangeTypeReturnPostprocessor ();

			var result = subject.Process (5, typeof(string));

			Assert.IsInstanceOf (typeof(string), result);
			Assert.AreEqual ("5", result);
		}

		[Test ()]
		public void WhenRequiredTypeIsDecimalReturnsDecimal()
		{
			var subject = new ChangeTypeReturnPostprocessor ();

			var result = subject.Process (5, typeof(decimal));

			Assert.IsInstanceOf (typeof(decimal), result);
			Assert.AreEqual (5m, result);
		}
	}
}

