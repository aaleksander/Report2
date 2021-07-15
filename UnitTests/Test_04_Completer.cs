using System;
using NUnit.Framework;
using RLisp.TextEditors;

namespace UnitTests
{
	[TestFixture]
	public class Test_04_Completer
	{
		[Test]
		public void Test_01_Stack()
		{
			var res = CompleterBase.GetContext("(page (def (tmp) (");
			Assert.AreEqual(res.Type, ContextType.LP);
			Assert.AreEqual(res.Stack.Count, 2);
			Assert.AreEqual(res.Stack.Pop().ToString(), "def");
			Assert.AreEqual(res.Stack.Pop().ToString(), "page");
		}

		[Test]
		public void Test_02_()
		{
			var c = new CompleterBase("(");
			c.AddData("", "par", "параграф");
			c.AddData("", "def", "определение функции или переменной");
			c.AddData("par", ":margin", "отступы");
			c.AddData("par", ":text-align", "выравнивание");
			c.AddData("cell", ":border", "граница");
			Assert.AreEqual(2, c.Data.Count); //в корень можно добавлять только параграф или def
			Assert.AreEqual("def", c.Data[0].Text);
			Assert.AreEqual("par", c.Data[1].Text);
		}

		[Test]
		public void Test_03_()
		{
			var c = new CompleterBase("(page (par ("); //что у нас можно использовать для параграфа
			c.AddData("", "page", "параграф");
			c.AddData("", "def", "определение функции или переменной");
			c.AddData("par", ":margin", "отступы");
			c.AddData("par", ":text-align", "выравнивание");
			c.AddData("cell", ":border", "граница");

			Assert.AreEqual(c.Data.Count, 2); //в корень можно добавлять только параграф или def
			Assert.AreEqual(c.Data[0].Text, ":margin");
			Assert.AreEqual(c.Data[1].Text, ":text-align");
		}

		[Test]
		public void Test_04_Stack()
		{
			string s = 
				"(page (:page-padding 2 1 1 1)" +
				"	  (:page-height 29.7)" +
				"	  (:page-width 21)" +
				"	  (:font-size 12)" +
				"	  (:font-family \"Times New Roman\")" +
				
				"	(par (:margin 30)" +
				"		\"ddsd sd\"" +
				"	)" +
				")" +
				"(page" +
				"	(";
			var res = CompleterBase.GetContext(s);

			Assert.AreEqual(res.Type, ContextType.LP);
			Assert.AreEqual(res.Stack.Count, 1);
			Assert.AreEqual(res.Stack.Pop().ToString(), "page");
		}
	}
}
