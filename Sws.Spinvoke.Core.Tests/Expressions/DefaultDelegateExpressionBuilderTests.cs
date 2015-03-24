using NUnit.Framework;
using System;

using Sws.Spinvoke.Core.Expressions;

namespace Sws.Spinvoke.Core.Tests
{
	[TestFixture ()]
	public class DefaultDelegateExpressionBuilderTests
	{
		public delegate int AddDelegate(int x, int y);

		[Test ()]
		public void BuildLinqExpressionCreatesLinqExpressionWrappingDelegate ()
		{
			var subject = new DefaultDelegateExpressionBuilder ();

			var addDelegate = new AddDelegate ((x, y) => x + y);

			var addFunc = subject.BuildLinqExpression<Func<int, int, int>> (addDelegate);

			var result = addFunc.Compile() (6, 7);

			Assert.AreEqual (13, result);
		}
	}
}

