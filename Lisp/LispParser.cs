using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lisp
{
	public static class LispParser
	{
		public static List<Tuple<int, char>> GetLevels(string aSource)
		{
			var res = new List<Tuple<int, char>>();
			int level = 0;
			bool calc = true;			
			foreach(var ch in Strip(aSource))
			{
				if( ch == '"' ) //между кавычками скобки не считаем 
				{
					calc = !calc;
				}

				if( ch == '(' && calc )
				{
					level++;
					res.Add(Tuple.Create<int, char>(level, ch));
					continue;
				}

				if( ch == ')' && calc)
				{
					res.Add(Tuple.Create<int, char>(level, ch));
					level--;
					continue;
				}

				res.Add(Tuple.Create<int, char>(level, ch));
			}

			return res;
		}

		public static List<LispTerm> GetTerms(string aSource)
		{
			var res = new List<LispTerm>();
			var levels = LispParser.GetLevels(aSource);

			return res;
		}

		public static LispTreeNode GetTree(string aSource)
		{			
			var st = new StateMachine();
			LispTreeNode root = new LispTreeNode();

			st.Cache = aSource;
			CommandType com = CommandType.None;
			while( com != CommandType.End )
			{
				var coms = st.NextChar();
				if( coms.Count > 1 )
					throw new Exception("Непонятно");//TODO Сделать отдельное исключение
				if( coms[0].Type == CommandType.BeginExpression )
				{
					root.Childs.Add(new LispTreeNode(ref st));
				}
			}
			return root;
		}

		/// <summary>
		/// удаляет лишние пробелы и переводы строки
		/// </summary>
		/// <param name="aSource"></param>
		/// <returns></returns>
		public static string Strip(string aSource, bool aFirst = true)
		{
			var sb = new StringBuilder();
			Func<char, bool> isWhite = x => x == ' ' || x == '\n';

			int whiteCnt = isWhite(aSource[0])? 1: 0; //счетчик пробелов
			char prev = aSource[0];
			foreach(var ch in aSource)
			{
				if( isWhite(ch) )
				{
					whiteCnt++;
					if( whiteCnt < 2)//&& prev != ')')
					{
						if( aFirst && prev != '(' )
							sb.Append(ch);
						if( !aFirst && prev != ')' )
							sb.Append(ch);						
					}
				}
				else
				{
					sb.Append(ch);
					whiteCnt = 0;
				}
				prev = ch;
			}

			//переворачиваем и проходим еще раз
			var rev = sb.ToString().Reverse();
			sb.Clear();
			foreach(var ch in rev)
			{
				sb.Append(ch);
			}

			if( aFirst )
			{
				return Strip(sb.ToString(), false);
			}
			return sb.ToString();
		}
	}
}
