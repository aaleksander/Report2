using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RLisp.TreeNodes;

namespace RLisp
{
	public static class Parser
	{
		public static TreeNodeBase GetTree(string aSource, Stack<TreeNodeBase> aStack = null)
		{	
			var root = new TreeNodeRoot(0, 0);//NodeType.Root);
			var st = new StateMachine();				
			st.Cache = aSource;
			CommandType com = CommandType.None;
			while( com != CommandType.End )
			{
				var coms = st.NextChar();
				if( coms.Count > 1 )
					throw new Exception("Непонятно");//TODO Сделать отдельное исключение
				if( coms.Count == 0 ) //команд нет, идем дальше
					continue;
				if( coms[0].Type == CommandType.BeginExpression )
				{
					root.Childs.Add(new TreeNodeExpression(st.Line, st.Col, ref st, aStack));
				}
				com = coms[0].Type;
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
