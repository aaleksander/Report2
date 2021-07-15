using System;
using RLisp;
using NUnit.Framework;
using System.Collections.Generic;
using RLisp.TreeNodes;

namespace UnitTests
{
	[TestFixture]
	public class Test_01_Parser
	{
		[Test]
		public void Test_01_Simple()
		{			// 111
			var src = "(a   1  )";

			var res = Parser.GetTree(src);
			Assert.AreEqual(res.Count, 1);
			Assert.IsTrue(res is TreeNodeRoot);

			Assert.IsTrue(res.Childs[0] is TreeNodeExpression);

			Assert.AreEqual(res.Childs[0].Childs[0].Source, "a");
			Assert.IsTrue(res.Childs[0].Childs[0] is TreeNodeIdent);
			Assert.AreEqual(res.Childs[0].Childs[0].Count, 0);

			Assert.AreEqual(res.Childs[0].Childs[1].Source, "1");
			Assert.IsTrue(res.Childs[0].Childs[1] is TreeNodeNumber);
		}


		public void Test_02_Recur()
		{			// 111
			var src = "(+ 1 (+ 2 3))\n"; //добавим перевод строки
					 // "(+ 4)";

			var res = Parser.GetTree(src);

			Assert.AreEqual(res.ToString(),
			                "Root\r\n" +
			                "  Expression = 6\r\n" +
			                "    +\r\n" +
			                "    val = 1\r\n" +
			                "    Expression = 5\r\n" +
			                "      +\r\n" +
			                "      val = 2\r\n" +
			                "      val = 3"
			               );
		}

		[Test]
		public void Test_03()
		{
			var src = "(def (f1 aa)\n" +
					  "  (* aa 2))\n" +
				      "(a 1 (f1 2))";
			var res = Parser.GetTree(src);
		}

		[Test]
		public void Test_04_String()
		{
			var src = @"(concat ""str"")(concat ""s1"" ""s2"")"; 
			var res = Parser.GetTree(src);
			Assert.AreEqual(res.ToString(),
			                "Root\r\n" +
			                "  Expression\r\n" +
			                "    concat\r\n" +
			                "    val = 'str'\r\n" +
			                "  Expression\r\n" +
			                "    concat\r\n" +
			                "    val = 's1'\r\n" +
			                "    val = 's2'"
			               );
		}

		[Test]
		public void Test_05_String()
		{
			var st = new StateMachine();
			var src =  @"(a (b 2) ""rrr"")";
//			           @"   (* 2 3)\n" +
//			           @"   (* 3 4)\n" +
//			           @"   ""конец"")";			
			var res = Parser.GetTree(src);
		}

		[Test]
		public void Test_06_()
		{
			var src =	"(def a 3)" +
			       		"(def (f1 b c) (+ b (* c 2)))" + //определение функции
			       		"(? (f1 2 a))"; //используем переменную
			var res = Parser.GetTree(src);
			Assert.AreEqual(res.ToString(),
			                "Root\r\n" +
			                "  Expression\r\n" +
			                "    def\r\n" +
			                "    a\r\n" +
			                "    val = 3\r\n" +
			                "  Expression\r\n" +
			                "    def\r\n" +
			                "    Expression\r\n" +
			                "      f1\r\n" +
			                "      b\r\n" +
			                "      c\r\n" +
							"    Expression\r\n" +
							"      +\r\n" +
							"      b\r\n" +
							"      Expression\r\n" +
							"        *\r\n" +
							"        c\r\n" +
							"        val = 2\r\n" +
							"  Expression\r\n" + 
							"    ?\r\n" +
							"    Expression\r\n" + 
							"      f1\r\n" + 
							"      val = 2\r\n" + 
							"      a"
			               );
		}

		[Test]
		public void Test_07_LispInString()
		{//игнорируе лисп команды внутри строк
			var i = Parser.GetTree("(page (par \"при(a) вет\"))");
		}

		[Test]
		public void Test_08_3Dot()
		{
//			(def (f1)
//				 (+ 1 (- 3 ...) )
//			)

			var t = Parser.GetTree(
			                       "(def (f1) " +
			                       "	 (+ 1 (- " +
			                       "		     3 " +
			                       "             ..." +
			                       "		  )" +
								   "	 )" +
								   ")").ToString();
			Assert.AreEqual(t, 			                
			                "Root\r\n" 				+
			                "  Expression\r\n" 		+
			                "    def\r\n" 			+
			                "    Expression\r\n" 	+
			                "      f1\r\n" 			+
			                "    Expression\r\n" 	+
			                "      +\r\n" 			+
			                "      val = 1\r\n" 	+ 
			                "      Expression\r\n" 	+
			                "        -\r\n" 		+ 
			                "        val = 3\r\n" 	+
			                "        dots"	);
		}

		[Test]
		public void Test_09_Stack()
		{//во время разбора, машина должна накапливать стэк текущих операций
			var stack = new Stack<TreeNodeBase>();
			Parser.GetTree(
				 		 "(page  \n" 			+
			             "	(def (tmp)\n" 		+
		 				 "    (cell\n" 			+
		 				 "      (par ...)\n"	+
		 				 "    )\n"				+
		 				 "	)\n"				+
		 				 ")", stack);
			Assert.AreEqual(stack.Count, 0); //программа полностью завершилась
		}

		[Test]
		public void Test_10_Stack()
		{//во время разбора, машина должна накапливать стэк текущих операций
			var stack = new Stack<TreeNodeBase>();
			try{
				Parser.GetTree(
				 		 "(page  \n" 			+
			             "	(def (tmp)\n" 		+
		 				 "    (cell\n" 			+
		 				 "      (par ...\n"
		 				 , stack);
			}catch(Exception){/* сюда попадем обязательно, потому что программа не законченна*/}
			Assert.AreEqual(stack.Count, 4); //программа хоть и не выполнится, но стэк должен быть заполнен
			Assert.AreEqual(stack.Pop().Source, "par");
			Assert.AreEqual(stack.Pop().Source, "cell");
			Assert.AreEqual(stack.Pop().Source, "def");
			Assert.AreEqual(stack.Pop().Source, "page");
		}
		
		[Test]
		public void Test_11_Stack()
		{//во время разбора, машина должна накапливать стэк текущих операций
			var stack = new Stack<TreeNodeBase>();
			try{
				Parser.GetTree(
				 		 "(page  \n" 			+
			             "	(def (tmp)\n" 		+
			             "    (+ 3 4)\n"		+
		 				 "    (cell\n"
		 				 , stack);
			}catch(Exception){/* сюда попадем обязательно, потому что программа не законченна*/}
			Assert.AreEqual(stack.Count, 3); //программа хоть и не выполнится, но стэк должен быть заполнен
			Assert.AreEqual(stack.Pop().Source, "cell");
			Assert.AreEqual(stack.Pop().Source, "def");
			Assert.AreEqual(stack.Pop().Source, "page");
		}
	}
}

