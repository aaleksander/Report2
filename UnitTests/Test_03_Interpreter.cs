using System;
using System.Globalization;
using System.Threading;
using RLisp;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RLisp.TreeNodes;

namespace UnitTests
{
	[TestFixture]
	public class Test_03_Interpreter
	{
		[SetUp]
		public void Setup()
		{
			CultureInfo clone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			clone.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = clone;
		}
			
		
		[Test]
		public void Test_01_Add()
		{
			var inter = new Interpreter();
			var res = inter.Eval("(+ 1 2)");

			Assert.IsTrue(res is int);

			Assert.AreEqual(res, 3);
		}

		[Test]
		public void Test_02_Mul()
		{
			var inter = new Interpreter();
			var res = inter.Eval("(* 2 3 4)");

			Assert.IsTrue(res is int);

			Assert.AreEqual(res, 24);
		}

		[Test]
		public void Test_02_Mul_Add()
		{
			var inter = new Interpreter();
			var res = inter.Eval("(* (+ 2 2) 2)");

			Assert.IsTrue(res is int);

			Assert.AreEqual(res, 8);
		}

		[Test]
		public void Test_03_Console()
		{
			var inter =  new Interpreter();
			var res = inter.Eval(@"(? ""123"")");

			Assert.AreEqual(inter.Console, "123");
		}

		[Test]
		public void Test_04_PrintValue()
		{
			var inter = new Interpreter();
			inter.ClearConsole();
			var res = inter.Eval("(? (+ 1 2))");
			Assert.AreEqual(inter.Console, "3");
		}
		
		[Test]
		public void Test_05_()
		{
			var inter = new Interpreter();
			inter.ClearConsole();

			inter.Eval("(? (+ 2 (* 2 3))" +
			           "   (* 3 4)\n" +
			           "   \"конец\")");
			Assert.AreEqual(inter.Console, "812конец");
		}

		[Test]
		public void Test_06_Concat()
		{//concatу пофиг что конкатенировать
			var inter = new Interpreter();
			inter.ClearConsole();
			inter.Eval("(? (concat 1 2 \"ss\"))");
			Assert.AreEqual(inter.Console, "12ss");
		}

		[Test]
		public void Test_07_DefVar()
		{
			var inter = new Interpreter();
			inter.ClearConsole();
			inter.Eval("(def a 23)");
			Assert.IsTrue(inter.Env.GetVar(0, 0, "a") != null );
			Assert.AreEqual(inter.Env.GetVar(0, 0, "a").Value , 23);
		}

		[Test]
		public void Test_08_UseVar()
		{
			var i = new Interpreter();
			i.ClearConsole();
			i.Eval("(def a 12)\n" +
			       "(? a)");
			Assert.AreEqual(i.Console, "12");
		}
		
		
		[Test]
		public void Test_09_AddVars()
		{
			var i = new Interpreter();
			i.ClearConsole();
			i.Eval("(def ab 12)\n" +
			       "(? (+ ab 3))");
			Assert.AreEqual(i.Console, "15");
		}

		[Test]
		public void Test_10_PushEnv()
		{
			var e = new RLisp.Environment();

			e.SetVar("a", 	new TreeNodeNumber(0, 0, "1"){
								Env = e
					         });
			Assert.AreEqual(e.GetVar(0, 0, "a").Value.ToString(), "1");

			e.Push(); //погружаемся
			e.SetVar("a", new TreeNodeNumber(0, 0, "2"));
			Assert.AreEqual(e.GetVar(0, 0, "a").Value.ToString(), "2");

			e.Pop();
			Assert.AreEqual(e.GetVar(0, 0, "a").Value.ToString(), "1");
		}

		[Test]
		public void Test_10_VarFromPush()
		{
			var e = new RLisp.Environment();

			e.SetVar("a", 	new TreeNodeNumber(0, 0, "1"){
								Env = e
					         });
			Assert.AreEqual(e.GetVar(0, 0, "a").Value.ToString(), "1");

			e.Push(); //погружаемся
			e.SetVar("a", new TreeNodeNumber(0, 0, "2"));
			Assert.AreEqual(e.GetVar(0, 0, "a").Value.ToString(), "2");

			e.Push(); //погружаемся
			e.SetVar("b", new TreeNodeNumber(0, 0, "3"));
			Assert.AreEqual(e.GetVar(0, 0, "b").Value.ToString(), "3");

			//тут мы должны пройти по стэку вверх и найти переменную
			Assert.AreEqual(e.GetVar(0, 0, "a").Value.ToString(), "2"); //взяли переменную из ближайшего стэка
		}

		[Test]
		public void Test_12_Func()
		{
			var i = new Interpreter();

			i.Eval("(def (f1 b c) (+ b (* c 2)))\n" +
			       "(? (f1 2 3))");
			Assert.AreEqual(i.Console, "8");
		}

		[Test]
		public void Test_13_FuncWithVar()
		{
			var i = new Interpreter();

			i.Eval("(def a 4)" +
			       "(def (f1 b c) (+ b c))" +
			       "(? (f1 2 a))"); //используем переменную
			Assert.AreEqual(i.Console, "6");
		}
		
		[Test]
		public void Test_14_PrintString()
		{
			var i = new Interpreter();

			i.Eval("(def center \"Center\")\n");
			i.Eval("(? center)");
			Assert.AreEqual(i.Console, "Center");
		}

		[Test]
		public void Test_15_FuncWithoutArgs()
		{
			var i = new Interpreter();

			i.Eval("(def (f1) (+ 1 4))\n" +
			       "(? (f1))");
			Assert.AreEqual(i.Console, "5");
		}

		[Test]
		public void Test_16_If()
		{//как if, только без then
			var i = new Interpreter();

			i.Eval("(def a 1)");
			i.Eval("(def b 4)");
			i.Eval("(if a (? (* b 2)))");
			Assert.AreEqual(i.Console, "8");
		}

		[Test]
		public void Test_17_IfElse()
		{
			var i = new Interpreter();

			i.Eval("(def (f1 a) (+ a 2))");
			var res = i.Eval("(if (f1 1) 3 else 4)");
			Assert.AreEqual(res, 3);
		}

		[Test]
		public void Test_18_JSONData()
		{
			var i = new Interpreter();

			string d = "{\n" +
                          "	a: 12, \n" +
                          "	b: 22, \n" +
                          "	arr:[12, 23, 34], \n" +
                          "	arrO:[\n" +
                          "		{x:91, y:92}, \n" +
                          "		{x:93, y:94} \n" +
                          "	], " +
						  "lng:{x:{y:{z:5678}}},"+
						  "}";

			i.SetJSON(d);

			var res = i.Eval("(json a)");
			Assert.AreEqual(res, 12);

			res = i.Eval("(+ 12 (json b))");
			Assert.AreEqual(res, 34);

			res = i.Eval("(json arr[1])");
			Assert.AreEqual(res, 23);

			res = i.Eval("(json arrO[1].y)");
			Assert.AreEqual(res, 94);

			res = i.Eval("(json lng.x.y.z)");
			Assert.AreEqual(res, 5678);
		}

		[Test]
		public void Test_19_JsonString()
		{
			var i = new Interpreter();

			string d = "{" +
                          "	arr:[\"12\", \"23\", \"34\"]" +
						  "}";

			i.SetJSON(d);

			var res = i.Eval("(? (json arr[0]))");
			Assert.AreEqual(i.Console, "12");
		}

		[Test]
		public void Test_20_List()
		{//цикл по массивую json
			var i = new Interpreter();

			var res = i.Eval("(list 1 2 5)");

			Assert.AreEqual(res, new int[]{1,2,5});
		}

		[Test]
		public void Test_21_Loop()
		{//цикл по массивую json
			var i = new Interpreter();

			i.Eval("(def a (list 1 2 5))");
			i.Eval("(loop i in a (? i \" - \"))");

			Assert.AreEqual(i.Console, "1 - 2 - 5 - ");

			//несколько команд в одном цикле
			i.ClearConsole();
			i.Eval("(loop i in (list 3 7 10) " +
			       "  (? (* i 2)) " +
			       "  (? \"; \")" +
			      ")");
			Assert.AreEqual(i.Console, "6; 14; 20; ");
		}

		[Test]
		public void Test_22_LoopFromJson()
		{
			var i = new Interpreter();
			var j = "{" +
				"x: 1," +
				"ar:[1,2,4]" +
				"}";
			i.SetJSON(j);

			i.Eval("(loop i in (json ar) " +
			       "	(? i) " +
			       "	(? \"+\"))");
			Assert.AreEqual(i.Console, "1+2+4+");
		}

		[Test]
		public void Test_23_Neg()
		{
			var i = new Interpreter();

			i.Eval("(def a -1)");
			i.Eval("(? (+ a 10))");
			Assert.AreEqual(i.Console, "9");
		}

		[Test]
		public void Test_24_Neg()
		{
			var i = new Interpreter();

			i.Eval("(def a 10)");
			i.Eval("(? -a)");
			Assert.AreEqual(i.Console, "-10");
		}

		[Test]
		public void Test_25_Minus()
		{
			var i = new Interpreter();
			var res = i.Eval("(- 10 2 3)");
			Assert.AreEqual((int)res, 5);
		}
		
		[Test]
		public void Test_26_Neg()
		{
			var i = new Interpreter();

			i.Eval("(? (- 0 (+ 1 5)))");
			Assert.AreEqual(i.Console, "-6");
		}

		[Test]
		public void Test_26_NewLine()
		{
			var i = new Interpreter();

			i.Eval("(? \"123\")");
			i.Eval("(new-line)");
			i.Eval("(? \"22\")");
			Assert.AreEqual(i.Console, "123\r\n22");
		}

		[Test]
		public void Test_26_NewLineInLoop()
		{
			var i = new Interpreter();
			i.Eval("(loop i in (list 11 2 3)" +
			       "  (? i (new-line)))");
			Assert.AreEqual(i.Console, "11\r\n2\r\n3\r\n");
		}

		[Test]
		public void Test_27_FuncMultiLine()
		{//функция из нескольких действий
			var i = new Interpreter();
			i.Eval("(def (f1 a b) " +
			       "     (? (+ a b)) " +
			       "     (? a b)" +
			       "     (* a b)" +
			       ")");
			var res = i.Eval("(f1 3 2)");
			Assert.AreEqual(res, 6);//возращается последний результат
			Assert.AreEqual(i.Console, "532");
		}

		[Test]
		public void Test_28_FuncMultiLine()
		{//функция из нескольких действий
			var i = new Interpreter();
			i.Eval("(def (f1 a b) " +
			       "     (? \"f1: \" a \" \" b) " +
			       "     (new-line)" +
			       "     (+ a b))" +			       
			       "(? (f1 1 2))");
			Assert.AreEqual(i.Console, "f1: 1 2\r\n3");
		}

		[Test]
		public void Test_29_ListOfString()
		{
			var i = new Interpreter();
			i.Eval("(def l (list \"a\" \"b\" \"c\"))" +
			       "(loop  i in l " +
				   "(? i)" +
				   ")");
			Assert.AreEqual(i.Console, "abc");
		}

		[Test]
		public void Test_30_Double()
		{
			var i = new Interpreter();
			i.Eval("(? 1.23)");
			Assert.AreEqual(i.Console, "1.23");
		}	

		[Test]
		public void Test_31_JsonArrayOfObject()
		{
			var i = new Interpreter();
			var j = "{arr: " +
					"	[ " +
					"		{x:1, y:2}, " +
					"		{x:3, y:4}, " +
					"		{x:5, y:6}" +
					"	]" +
					"}	";
			i.SetJSON(j);

			i.Eval("(loop d in (json arr) (? d.x d.y))");
			Assert.AreEqual(i.Console, "123456");
		}

		[Test]
		public void Test_32_jsonArrayInArray()
		{
			var j = "{ " +
				"arr:[" +
				"	{y: 30, z:[1,2,3]}," +
				"	{y: 40, z:[4,5,6]}," +
				"	{y: 50, z:[7,8,9]}," +
				"	{y: 60, z:[10,11,12]}," +
				"	{y: 70, z:[13,14,15]}," +
				"	]" +
				"	}";
			var i = new Interpreter();
			i.SetJSON(j);
			i.Eval(
				"(loop a in (json arr)" +
				"	(? a.y \"_\")" +
				"	(loop zz in a.z" +
				"		(? zz)" +
				"	)" +
				")");
			Assert.AreEqual("30_12340_45650_78960_10111270_131415", i.Console);
		}

		[Test]
		public void Test_33_Equal()
		{
			var i = new Interpreter();

			var res = i.Eval("(def a 3)(if (= a 3) 1 2)");
			Assert.AreEqual(res, 2); //получаем последнее значение в последовательности

			res = i.Eval("(def a 1)(if (= a 3) 1 else 10)");
			Assert.AreEqual(res, 10);
		}

		[Test]
		public void Test_34_Carring()
		{
			var i = new Interpreter();
			var res = i.Eval(
				"(def (a) (? \"carr: \"))" + //определяем функцию, которая перед любым вызовом "?" пишет "car:"
				"(a \"1\" \"2\")"
			);
			Assert.AreEqual(i.Console, "carr: 12");

			i.ClearConsole();
			i.Eval("(? 44)");
			Assert.AreEqual(i.Console, "44");

			i.ClearConsole();
			i.Eval("(a 44)");
			Assert.AreEqual(i.Console, "carr: 44");
		}

		[Test]
		public void Test_35_if()
		{
			var i = new Interpreter();
			i.Eval("(if 1 (? \"1111\") (? \"-222\"))");
			Assert.AreEqual(i.Console, "1111-222");
		}

		[Test]
		public void Test_36_if_else_miltiline()
		{
			var i = new Interpreter();
			i.Eval("(if 0 (? \"1111\") (? \"3\") else (? \"222\") (? \"-123\"))");
			Assert.AreEqual(i.Console, "222-123");
		}

		[Test]
		public void Test_37_if_great()
		{
			var i = new Interpreter();
			i.Eval("(if (> 3 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");
			i.ClearConsole();
			i.Eval("(if (> 1 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "2");
		}

		[Test]
		public void Test_38_if_less()
		{
			var i = new Interpreter();
			i.Eval("(if (< 3 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "2");
			i.ClearConsole();
			i.Eval("(if (< 1 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");
		}
		

		[Test]
		public void Test_39_if_GreatEqual()
		{
			var i = new Interpreter();
			i.Eval("(if (>= 3 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");

			i.ClearConsole();
			i.Eval("(if (>= 3 3) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");

			i.ClearConsole();
			i.Eval("(if (>= 2 3) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "2");
		}

		[Test]
		public void Test_40_if_LessEqual()
		{
			var i = new Interpreter();
			i.Eval("(if (<= 3 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "2");

			i.ClearConsole();
			i.Eval("(if (<= 3 3) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");
			
			i.ClearConsole();
			i.Eval("(if (<= 2 3) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");

		}

		[Test]
		public void Test_41_if_notEqual()
		{
			var i = new Interpreter();
			i.Eval("(if (!= 3 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "1");
			i.ClearConsole();
			i.Eval("(if (!= 2 2) (? \"1\") else (? \"2\"))");
			Assert.AreEqual(i.Console, "2");
		}
		
		[Test]
		public void Test_42_3dot()
		{
			var i = new Interpreter();

			i.Eval(
			       "	(def (tmp)\n" 	+
		 		   "    (cell\n" 		+
		 		   "      (par ...)\n" 	+
		 		   "    )\n" 			+ 
		 		   "  )\n" 				);
		}
		
		
		[Test]
		public void Test_43_3Dot()
		{
			var i = new Interpreter();
			i.Eval(
			       "(def (tmp)				" +
			       "	 (? \"begin\" ...)	" + //параметры должны попасть именно сюда
			       "	 (? \"end\")		" +
			       ")						" +
			       "(tmp \"1\" \"2\")		"
			      );
			Assert.AreEqual(i.Console, "begin12end");
		}

		[Test]
		public void Test_44_3DotRecur()
		{
			var i = new Interpreter();
			i.Eval(
			       "(def (tmp)					" +
			       "	 (loop i in (list 1 2)	" + 
			       "	   (? i ... )			" + //три точки находятся рекурсивно
			       "     )						" + 
			       "	 (? \"end\")			" +
			       ")							" +
			       "(tmp \"3\" \"4\")			"
			      );
			Assert.AreEqual(i.Console, "134234end");
		}

		[Test]
		public void Test_45_()
		{
			var i = new Interpreter();
			i.Eval(
			       "(def (tmp)					" +
			       "	 (? \"1\")			" + //три точки находятся рекурсивно
			       "	 (? \"end\")			" +
			       ")							" +
			       "(tmp \"3\" \"4\")			"
			      );
			Assert.AreEqual(i.Console, "1end34");
		}

		[Test]
		public void Test_46_JsonWithVar()
		{
			var i = new Interpreter();
			i.SetJSON("{arr:[12,23,34,45,56]}");
			i.Eval(
				"(def a 3)" +
				"(? (json arr[$a]))");
			Assert.AreEqual(i.Console, "45");
		}

		[Test]
		public void Test_47_JsonWithVar()
		{
			var i = new Interpreter();
			i.SetJSON("{arr:[12,23,34,45,56]}");
			i.Eval(
				"(def (f1 a) (? (json arr[$a])))" +
				"(f1 2)");
			Assert.AreEqual(i.Console, "34");
		}

		[Test]
		public void Test_48_JsonWithVar()
		{
			var i = new Interpreter();
			i.SetJSON("{arr:[{x:12,y:23},{x:34,y:45}]}");
			i.Eval(
				"(def (f1 x) (? (json arr[0].$x)))" +
				"(f1 \"y\")");
			Assert.AreEqual(i.Console, "23");
		}
		
		[Test]
		public void Test_49_Or()
		{
			var i = new Interpreter();
			Assert.AreEqual(i.Eval("(or 1 1 0 )"), 1);
			Assert.AreEqual(i.Eval("(or 0 1 0 0)"), 1);
			Assert.AreEqual(i.Eval("(or (+ 1 1) 0 1)"), 1);
			Assert.AreEqual(i.Eval("(or 0 0)"), 0);
			Assert.AreEqual(i.Eval("(or 0 0 0 0)"), 0);
		}
		
		[Test]
		public void Test_50_And()
		{
			var i = new Interpreter();
			Assert.AreEqual(i.Eval("(and 1 1 0 )"), 0);
			Assert.AreEqual(i.Eval("(and 0 1 0 0)"), 0);
			Assert.AreEqual(i.Eval("(and 1 0 1)"), 0);
			Assert.AreEqual(i.Eval("(and 0 0)"), 0);
			Assert.AreEqual(i.Eval("(and 1 1 1 1)"), 1);
		}
		
		[Test]
		public void Test_51_Not()
		{
			var i = new Interpreter();
			Assert.AreEqual(i.Eval("(not 1)"), 0);
			Assert.AreEqual(i.Eval("(not 0)"), 1);
			Assert.AreEqual(i.Eval("(not 11)"), 0);
		}

		[Test]
		public void Test_52_Set()
		{
			var i = new Interpreter();
			i.Eval("(def a 3)");
			i.Eval("(? a)");
			Assert.AreEqual(i.Console, "3");
			i.ClearConsole();
			i.Eval("(set! a 67)");
			i.Eval("(? a)");
			Assert.AreEqual(i.Console, "67");
		}

		[Test]
		public void Test_53_Set()
		{
			var i = new Interpreter();
			i.Eval("(def a \"aa\")");
			i.Eval("(? a)");
			Assert.AreEqual(i.Console, "aa");
			i.ClearConsole();
			i.Eval("(set! a \"vc\")");
			i.Eval("(? a)");
			Assert.AreEqual(i.Console, "vc");
		}

		[Test]
		[ExpectedException(typeof(BaseException))]
		public void Test_54_Set()
		{//пытаемся присвоить значение переменной
			var i = new Interpreter();
			i.Eval("(def (a) (? 3))");
			i.Eval("(set! a \"vc\")");

			Assert.AreEqual(i.Console, "vc");
		}

		[Test]
		public void Test_55_Set()
		{
			var src = 	"(def dd 3)" +
						"(def (ff)" +
						"    (set! dd 3) " + //он должен искать вверх по окружению
						")" +
						"(ff)";
			var i = new Interpreter();
			i.Eval(src);
		}

		[Test]
		public void Test_56_BadJson()
		{//пытаемся получить несуществующий путь в json
			var i = new Interpreter();

			string d = "{\n" +
                          " a: 12, \n" +
						  "}";
			i.SetJSON(d);

			try
			{
				var res = i.Eval("(json b)");
				Assert.IsTrue(false, "исключение не наступило");
			}catch(BaseException e)
			{
				Assert.AreEqual("путь в json не найден: line=1; col=7"
				               , e.Message);
			}
			catch(Exception)
			{
				Assert.IsTrue(false, "исключение не то");
			}
		}
		
		[Test]
		public void Test_57_BadJson()
		{//как 56-ой, но в цикле
			var i = new Interpreter();

			string d = "{\n" +
				"arr:[" +
                          " {a: 12}, {a: 10} \n" +
						  "]}";
			i.SetJSON(d);

			try
			{
				var res = i.Eval("(loop i in (json arr) (? i.b))");
				Assert.IsTrue(false, "исключение не наступило");
			}catch(BaseException e)
			{
				Assert.AreEqual("переменная i.b не определена: line=1; col=26"
				               , e.Message);
			}
		}

//		[Test]
		public void Test_99_Error_Empty()
		{
			var inter = new Interpreter();
			try
			{
				var res = inter.Eval("(+ () 1 2)");
				Assert.IsTrue(false, "мы не должны были до сюда дойти");
			}catch(EpmtyExpressionException e)
			{
				Assert.AreEqual(e.Line, 1);
				Assert.AreEqual(e.Col, 3);
			}			
		}
	}
}
