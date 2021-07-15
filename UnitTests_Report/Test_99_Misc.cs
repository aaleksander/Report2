/*
 * Сделано в SharpDevelop.
 * Пользователь: User
 * Дата: 10.11.2015
 * Время: 16:56
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using NUnit.Framework;
using RLisp.TextEditors;

namespace UnitTests_Report
{
	[TestFixture]
	public class Test_99_Misc: Test_00
	{
		[Test]
		public void Test_01_PairBracket()
		{
			var str = "(def (a b))";
			Assert.AreEqual(BracketHighlightRenderer.FindPairBracket(str, 5), 9); //внутренние скобки, встали на "("
			Assert.AreEqual(BracketHighlightRenderer.FindPairBracket(str, 9), 5); //внутренние скобки, встали на ")"
			
			Assert.AreEqual(BracketHighlightRenderer.FindPairBracket(str, 0), 10);
		}

		[Test]
		public void Test_02_UpDown()
		{
			var i = GetI();
			i.Eval("(up-down 20 10 12 \"123\")");
			Assert.AreEqual(i.Console, "<Run FontSize=\"20\" BaselineAlignment=\"Baseline\">123</Run>");
		}

		
		
		
		[Test]
		public void Test_03_Round()
		{
			var src = "(format (:round) 1.23)"; //(:after-zero 3) (:separator \",\")
			var i = GetI();
			var res = i.Eval(src);
			Assert.AreEqual(1, res);
		}

		[Test]
		public void Test_04_AfterZero()
		{
			var i = GetI();
			Assert.AreEqual("1.230", i.Eval("(format (:after-dot 3) 1.23)"), "1");
			Assert.AreEqual("1.23", i.Eval("(format (:after-dot 2) 1.234)"), "2");
			Assert.AreEqual("1.23", i.Eval("(format (:after-dot 2) 1.235)"), "3");
		}

		[Test]
		public void Test_05_AfterZeroAndRound()
		{
			var i = GetI();
			Assert.AreEqual("1.230", i.Eval("(format (:after-dot 3) (:round) 1.23)"), "1");
			Assert.AreEqual("1.23", i.Eval("(format (:after-dot 2) (:round) 1.234)"), "2");
			Assert.AreEqual("1.24", i.Eval("(format (:after-dot 2) (:round) 1.235)"), "3");
			Assert.AreEqual("1.235", i.Eval("(format (:after-dot 3) (:round) 1.235)"), "4");
		}
		
		[Test]
		public void Test_06_Separator()
		{
			var i = GetI();
			Assert.AreEqual("1", i.Eval("(format (:round) (:sep \",\") 1.23)"), "1");
			Assert.AreEqual("1,23", i.Eval("(format (:after-dot 2) (:round) (:sep \",\") 1.234)"), "2");
			Assert.AreEqual("1,24", i.Eval("(format (:after-dot 2) (:round) (:sep \",\") 1.235)"), "3");
			Assert.AreEqual("1,235", i.Eval("(format (:after-dot 3) (:round) (:sep \",\") 1.235)"), "3");
		}
	}
}
