using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using RLisp;
using NUnit.Framework;
using RLisp.TreeNodes;

namespace UnitTests
{
	[TestFixture]
	public class Test_02_StateMachine
	{
		[SetUp]
		public void Init()
		{
			CultureInfo clone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			clone.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = clone;
		}

		[Test]
		public void Test_01_Init()
		{
			var st = new StateMachine();
			Assert.AreEqual(st.State, StateMachine.StateType.WaitLP);
			Assert.AreEqual(st.Line, 1);
			Assert.AreEqual(st.Col, 0);
		}

		[Test]
		public void Test_02_WaitIdentifier()
		{
			var st = new StateMachine();
			st.NextChar(' ');
			Assert.AreEqual(st.Line, 1);
			Assert.AreEqual(st.Col, 1);
			st.NextChar('\n');
			Assert.AreEqual(st.Line, 2);
			Assert.AreEqual(st.Col, 0);
			st.NextChar('(');
			Assert.AreEqual(st.State, StateMachine.StateType.WaitIdentifier);
		}

		[Test]
		[ExpectedException(typeof(NoLPException))]
		public void Test_03_NoLP()
		{
			var st = new StateMachine();

			st.NextChar(' ');
			st.NextChar('\n');
			st.NextChar('a');
		}

		[Test]
		[ExpectedException(typeof(NoIdentException))]
		public void Test_04_NoIdent()
		{
			var st = new StateMachine();

			st.NextChar('(');
			st.NextChar('3');
		}

		[Test]
		public void Test_04_ReadIdent()
		{
			var st = new StateMachine("(abc");

			Assert.AreEqual(st.State, StateMachine.StateType.ReadingIdent);
		}

		[Test]
		public void Test_05_Ident() 
		{
			var st = new StateMachine("");
			List<Command> com = st.NextChar('(');
			Assert.AreEqual(com[0].Type, CommandType.BeginExpression);
			st.NextChar('a');
			st.NextChar('b');
			com = st.NextChar(' ');
			Assert.AreEqual(com[0].Type, CommandType.Ident);
			Assert.AreEqual(com[0].Argument, "ab");
			Assert.AreEqual(st.State, StateMachine.StateType.WaitArgument);
		}

		[Test]
		public void Test_06_End()
		{
			var st = new StateMachine("(a 1");
			var com = st.NextChar(')');
			Assert.AreEqual(com[0].Type, CommandType.Number);
			Assert.AreEqual(com[1].Type, CommandType.EndExpression);
		}

		[Test]
		public void Test_07_All()
		{ //(a 1 2.34)
			//string buff = "";
			var st = new StateMachine();
			st.Cache = "(a 1 2.34)";
			
			var com = st.NextChar(); //a
			Assert.AreEqual(com[0].Type, CommandType.BeginExpression);

			com = st.NextChar(); // ' '
			Assert.AreEqual(com.Count, 0);//[0].Type, CommandType.None);
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingIdent);

			com = st.NextChar(); // '1'
			Assert.AreEqual(com[0].Type, CommandType.Ident);
			Assert.AreEqual(com[0].Argument, "a");

			com = st.NextChar(); // ' '
			Assert.AreEqual(com.Count,  0);//CommandType.None);
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingNumber);

			com = st.NextChar(); //' '
			Assert.AreEqual(com[0].Type, CommandType.Number);
			Assert.AreEqual(com[0].Argument, "1");

			com = st.NextChars(4); //2.34
			Assert.AreEqual(com.Count, 0);// CommandType.None);
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingNumber);
			
			com = st.NextChar(); // ')'
			Assert.AreEqual(com[0].Type, CommandType.Number);
			Assert.AreEqual(com[0].Argument, "2.34");
			Assert.AreEqual(com[1].Type, CommandType.EndExpression);
			Assert.AreEqual(st.State, StateMachine.StateType.WaitLP);
			
		}

		[Test]
		[ExpectedException(typeof(NumberIsWrongException))]
		public void Test_08_WrongNumber()
		{
			new StateMachine("(a 2.333.4)");
		}

		[Test]
		public void Test_09_QueueCommands()
		{//очередь команд достуна только с использованием кэша
			var st = new StateMachine();
			st.Cache = "(a 1)";
			var com = st.NextChars(3); // "(a "
			Assert.AreEqual(st.State, StateMachine.StateType.WaitArgument);
			Assert.AreEqual(com.Count, 2);
			Assert.AreEqual(com[1].Type, CommandType.Ident);

			com = st.NextChars(2); // "1)"
			Assert.AreEqual(com.Count, 2);
			Assert.AreEqual(com[0].Type, CommandType.Number);
			Assert.AreEqual(com[0].Argument, "1");
			Assert.AreEqual(com[1].Type, CommandType.EndExpression);
		}

		[Test]
		public void Test_10_IdentWithNumber()
		{
			var st = new StateMachine();
			st.NextChar('(');
			st.NextChar('a');
			st.NextChar('1');
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingIdent);
			var com = st.NextChar(' ');
			Assert.AreEqual(st.State, StateMachine.StateType.WaitArgument);
			Assert.AreEqual(com[0].Type, CommandType.Ident);
			Assert.AreEqual(com[0].Argument, "a1");
		}

		[Test]
		[ExpectedException(typeof(MachRPException))]
		public void Test_11_ManyRP()
		{
			var st = new StateMachine();
			st.NextChars("(a 1 2))");
		}

		[Test]
		public void Test_12_NoManyRP()
		{
			var st = new StateMachine();
			st.NextChars("(def (f1 a)\n" +
			             "  (* a 2))\n" +
			             "(b 1 (f1 2))");
			//st.NextChars("(a 1 2 (b 1))");
		}

		[Test]
		public void Test_13_Neg()
		{
//			var st = new StateMachine();
//			st.NextChars("(a (- 1))");
//			Assert.AreEqual(st.State, StateMachine.StateType.ReadingNumber);
		}
		
		[Test]
		public void Test_14_String()
		{
			var st = new StateMachine();
			st.NextChars(@"(a ""тест");
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingString);
			var com = st.NextChar('"');
			Assert.AreEqual(com[0].Type, CommandType.String);
			Assert.AreEqual(com[0].Argument, "тест");
		}
		
		[Test]
		public void Test_15_NumberAsString()
		{
			var st = new StateMachine();
			st.NextChars(@"(a ""123");
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingString);
			var com = st.NextChar('"');
			Assert.AreEqual(com[0].Type, CommandType.String);
			Assert.AreEqual(com[0].Argument, "123");
		}
		
		[Test]
		public void Test_16_IdentWithMinus()
		{
			var st = new StateMachine();
			st.NextChars(@"(a-");
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingIdent);
			var com = st.NextChars("b ");
			Assert.AreEqual(com[0].Type, CommandType.Ident);
			Assert.AreEqual(com[0].Argument, "a-b");
			
		}
		
		
		[Test]
		public void Test_17_StringWithDot()
		{
			var st = new StateMachine();
			st.NextChars("(? \"a.");
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingString);
			var com = st.NextChars("b\" ");
			Assert.AreEqual(com[0].Type, CommandType.String);
			Assert.AreEqual(com[0].Argument, "a.b");
			
		}

		[Test]
		public void Test_18_IsoleteKav()
		{
			var st = new StateMachine();
			st.NextChars("(? \"a\\");
			Assert.IsTrue(st.Isolete);
			st.NextChars("\"");
			Assert.IsFalse(st.Isolete); //на следующем символы изоляция отключается
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingString);
		}
		
		[Test]
		public void Test_19_LispInString()
		{//игнорируе лисп команды внутри строк
			var st = new StateMachine();
			st.NextChars("(def f \"123(");
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingString);
			
		}
		
		[Test]
		public void Test_20_LispInString()
		{//игнорируе лисп команды внутри строк
			var st = new StateMachine();
			st.NextChars("(def f \"123(a )");
			Assert.AreEqual(st.State, StateMachine.StateType.ReadingString);
		}
		
		[Test]
		public void Test_21_OneLineComment()
		{
			var st = new StateMachine();
			st.NextChars("(def a 22)\n;222");
			Assert.AreEqual(st.State, StateMachine.StateType.OneLineComment);
		}

		[Test]
		public void Test_22_3Dot()
		{
			var st = new StateMachine();
			st.NextChars("(a .");
			Assert.AreEqual(st.State, StateMachine.StateType.Dots);
			st.NextChar('.');
			Assert.AreEqual(st.State, StateMachine.StateType.Dots);
			var com = st.NextChar('.');
			Assert.AreEqual(st.State, StateMachine.StateType.WaitSpace);
			Assert.AreEqual(com[0].Type, CommandType.Dots);
		}

		[Test]
		public void Test_23_3Dot()
		{
			var st = new StateMachine();
			st.NextChars("(a ....");
		}
		
		[Test]
		public void Test_24_3Dot()
		{
			var st = new StateMachine();
			var com = st.NextChars("(page  \n" +
			             "	(def (tmp)\n" 	+
		 				 "    (cell\n" 		+
		 				 "      (par ...)");//\n" 	+
			Assert.AreEqual(com.Last().Type, CommandType.EndExpression);
		}


		
		
//		[Test]
		public void Test_25_ExpressionWithMinus()
		{//модификатор "-" для выражения
			//надо его как-то приспособить к выражению, а то теряется
			var src = "(+ -(+ 1 2) 3)";
			var st = new StateMachine();
			st.NextChars("(+ -(+ 1 2) 3)");
			var s = Parser.GetTree(src);
//			st.NextChars("(");
//			Assert.AreEqual(st.State, StateMachine.StateType.WaitLP);;
		}
	}
}
