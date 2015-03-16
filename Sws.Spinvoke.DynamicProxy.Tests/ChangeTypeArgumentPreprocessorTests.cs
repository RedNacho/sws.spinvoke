using NUnit.Framework;
using System;
using System.Linq;

namespace Sws.Spinvoke.DynamicProxy.Tests
{
	[TestFixture ()]
	public class ChangeTypeArgumentPreprocessorTests
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

			var canProcess = conversionPairs.Select(pair => new ChangeTypeArgumentPreprocessor(pair.Item1).CanProcess(pair.Item2)).All(result => result);

			Assert.IsTrue (canProcess);
		}

		[Test ()]
		public void WhenRequiredTypeIntProcessReturnsInt()
		{
			var subject = new ChangeTypeArgumentPreprocessor (typeof(int));

			const decimal Original = 4m;

			var processed = subject.Process (Original);

			Assert.IsInstanceOf (typeof(int), processed);
			Assert.AreEqual (4, processed);
		}

		[Test ()]
		public void WhenRequiredTypeStringProcessReturnsString()
		{
			var subject = new ChangeTypeArgumentPreprocessor (typeof(string));

			const decimal Original = 5m;

			var processed = subject.Process (Original);

			Assert.IsInstanceOf (typeof(string), processed);
			Assert.AreEqual ("5", processed);
		}
	}
}

