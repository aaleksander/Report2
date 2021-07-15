using System;
using NUnit.Framework;
using RLisp;

namespace UnitTests
{
	[TestFixture]
	public class Test_05_Accumulator
	{
		[Test]
		public void Test_01()
		{
			var a = new Accumulator();
			Assert.IsTrue(a.Add(1) is int, "1");
			Assert.IsTrue(a.Add(1.3) is double, "2");
			Assert.AreEqual(a.Value, 2.3, "3");				
		}
	}
}
