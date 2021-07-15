/*
 * Сделано в SharpDevelop.
 * Пользователь: aaleksander
 * Дата: 23.10.2015
 * Время: 21:27
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using Editor.ReportDSL;
using NUnit.Framework;
using RLisp;

namespace UnitTests_Report
{
	[TestFixture]
	public class Test_01_Paragraph: Test_00
	{
		[Test]
		public void Test_01_Simple()
		{
			var i = GetI();
			i.Eval("(par)");
			Assert.AreEqual(i.Console, "<Paragraph ></Paragraph>");
		}

		[Test]
		public void Test_02_Text()
		{
			var i = GetI();
			i.Eval("(par \"123\")");
			Assert.AreEqual(i.Console, "<Paragraph >123</Paragraph>");
		}
		
		[Test]
		public void Test_02_Text_2()
		{
			var i = GetI();
			i.Eval("(par '123')");
			Assert.AreEqual(i.Console, "<Paragraph >123</Paragraph>");
		}

		[Test]
		public void Test_03_Vars()
		{
			var i = GetI();
			i.Eval("(def a 333)");
			i.Eval("(par a)");
			Assert.AreEqual(i.Console, "<Paragraph >333</Paragraph>");
		}

		[Test]
		public void Test_04_Margin()
		{
			var i = GetI();
			i.Eval("(par (:margin 0 1 1 0) \"321\" 5)");
			Assert.AreEqual(i.Console, "<Paragraph Margin=\"0,1,1,0\" >3215</Paragraph>");
		}
	
	[Test]
		public void Test_05_Func()
		{
			var i = GetI();
			i.Eval("(def a1 56)");
			i.Eval("(par (:margin 0 1 1 0) \"321\" \"_\" a1)");
			Assert.AreEqual(i.Console, "<Paragraph Margin=\"0,1,1,0\" >321_56</Paragraph>");
		}

		[Test]
		public void Test_06_NewLine()
		{//перевод строки в середине параграфа
			var i = GetI();
			i.Eval("(par \"12\" (new-line) \"56\")");
			Assert.AreEqual(i.Console, "<Paragraph >12\r\n56</Paragraph>");
		}

		[Test]
		public void Test_07_vars2()
		{//переменная в качестве параметра форматирования
			var i = GetI();
			i.Eval("(def a 3)");
			i.Eval("(par (:margin 1 0 0 a) \"12\" (new-line) \"56\")");
			Assert.AreEqual(i.Console, "<Paragraph Margin=\"1,0,0,3\" >12\r\n56</Paragraph>");
		}

		[Test]
		public void Test_08_FuncInside()
		{//функция внутри параграфа
			var i = GetI();
			i.Eval("(def (f1 a) (* a a))");
			i.Eval("(par \"рес: \" (f1 11))");
			Assert.AreEqual(i.Console, "<Paragraph >рес: 121</Paragraph>");
		}

		[Test]
		public void Test_09_TextAlign()
		{
			var i = GetI();
			i.Eval("(par (:text-align center) \"123\")");
			Assert.AreEqual(i.Console, "<Paragraph TextAlignment=\"Center\" >123</Paragraph>");
		}

		[Test]
		public void Test_10_LineHeight()
		{
			var i = GetI();
			i.Eval("(par (:line-height 12) \"123\")");
			Assert.AreEqual(i.Console, "<Paragraph LineHeight=\"12\" >123</Paragraph>");
		}

		[Test]
		public void Test_11_FontFamily()
		{
			var i = GetI();
			i.Eval("(par (:font-family \"Arial\") \"123\")");
			Assert.AreEqual(i.Console, "<Paragraph FontFamily=\"Arial\" >123</Paragraph>");
		}

		[Test]
		public void Test_11_FontSize()
		{
			var i = GetI();
			i.Eval("(par (:font-size 10) \"123\")");
			Assert.AreEqual(i.Console, "<Paragraph FontSize=\"10\" >123</Paragraph>");
		}

		[Test]
		public void Test_12_SomeParams()
		{
			var i = GetI();
			i.Eval("(par (:font-size 10) (:margin 4) \"123\")");
			Assert.AreEqual(i.Console, "<Paragraph FontSize=\"10\" Margin=\"4\" >123</Paragraph>");
		}

		[Test]
		public void Test_13_Json()
		{
			string j = "{a:1,}";
			var i = GetI();
			i.SetJSON(j);

			i.Eval("(par (json a))");
			Assert.AreEqual(i.Console, "<Paragraph >1</Paragraph>");
		}
		
		[Test]
		public void Test_14_()
		{
			var i = GetI();

			i.Eval("(par (+ 1 2))");
			Assert.AreEqual(i.Console, "<Paragraph >3</Paragraph>");
		}
	}
}
