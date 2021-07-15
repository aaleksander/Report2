using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lisp
{
	/// <summary>
	/// Узел дерева распарсенного исходника
	/// </summary>
	public class LispTreeNode
	{
		public LispTreeNode() //это конструктор для рута
		{
			Childs = new List<LispTreeNode>();
		}
		
		public LispTreeNode(string aS) //это обычный аргрумент
		{
			Source = aS;
		}

		public LispTreeNode(ref StateMachine st)
		{//Если мы здесь, значит StateMachine уже напоролась на '('
			Childs = new List<LispTreeNode>();
			List<Command> coms = new List<Command>();
			string str;
			var com = new Command(){
				Type = CommandType.None
			};
			while( com.Type != CommandType.EndExpression )
			{
				foreach(var c in coms)
				{
					com = c;
					switch(com.Type)
					{
						case CommandType.End:
							//TODO сделать нормальное прервывание
							throw new Exception("Неожиданное завершение программы");
						case CommandType.None:
							break;
						case CommandType.Ident:
							str = com.Argument;
							Childs.Add(new LispTreeNode(str));
							break;
						case CommandType.Number:
							str = com.Argument;
							Childs.Add(new LispTreeNode(str));
							break;
						case CommandType.BeginExpression:
							break;
						case CommandType.EndExpression:
							//тут должен быть последний аргрумент
							str = com.Argument;
							Childs.Add(new LispTreeNode(str));
							return; ///создание этого узла законченно
					}
				}
				coms = st.NextChar();
			}
			return;

//			string str = "";
//			Source = aSource;
//
//			var l = LispParser.GetLevels(aSource);
//
//			if( l[0].Item2 != '(' ) //это простая переменная
//			{
//
//				return;
//			}
//
//			var parts = new List<Tuple<Tuple<int, char>, Tuple<int,char>>>();
//
//			Tuple<int, char> end;
//
//			try
//			{
//				end = l.First(x => x.Item1 == 1 && x.Item2 == ')');
//			}
//			catch(InvalidOperationException)
//			{
//				throw new Exception("Не могу найти закрывающую скобку");
//			}
//
//			int iEnd = l.IndexOf(end);
//			//Склеивает текст обратно
//			StringBuilder sb = new StringBuilder();
//			for(int i=0; i <= iEnd; i++)
//			{
//				sb.Append(l[i].Item2);
//			}
//
//			//убираем скобочки спереди и сзади
//			str = sb.ToString().Substring(1, sb.ToString().Length - 2);
//
//			var pars = str.Split(' '); //делим на части
//
//			Childs = new List<LispTreeNode>();
//			foreach(var p in pars)
//			{
//				Childs.Add(new LispTreeNode(p));
//			}
		}

		public List<LispTreeNode> Childs{set;get;}

		public string Source{private set;get;}

		public int Count{
			get{
				return Childs.Count;
			}
		}
	}
}
