using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RLisp.ReportDSL;
using RLisp.TreeNodes;

namespace RLisp
{
	/// <summary>
	/// представление цикла
	/// (loop i for (list 123) (...(? i)) (? (+ i 2)))
	/// </summary>
	[Description("цикл: (loop a in (list 1 2 3) (...))")]
	public class LoopOperator: IOperator
	{
		public bool IsReturn{get{return false;}}
		public string Alias{get{return "loop";}}

		public LoopOperator()
		{
			
		}

		public string Name{private set; get;}
		private List<Tuple<string, TreeNodeBase>> _params = new List<Tuple<string, TreeNodeBase>>(); //список параметров

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length < 4 )
				throw new Exception("количество параметров цикла неверно (loop i in (list ...) (...))");

			if( !(aPars[0] is TreeNodeIdent) )
				throw new Exception("вторым параметром цикл должна идти переменная");

			if( !(aPars[1] is TreeNodeIdent) || (aPars[1] as TreeNodeIdent).Source != "in")
				throw new Exception("неверная конструкция loop. нужно так: (loop i in (list ...) (...)) ");

			var _iter = aPars[0].Source;
			var _enumerator = aPars[2];
			var _commands = aPars.Skip(3).ToList(); //забираем команды

			_enumerator.Env = aEnv;
			aEnv.Push();
			var l = _enumerator.Value as object[];

			foreach(var i in l)
			{
				if( i is int || i is double || i is Int64 || i is UInt64)
				{
					aEnv.SetVar(_iter, new TreeNodeNumber(0, 0, i.ToString()));
				}
				if( i is string )
				{
					aEnv.SetVar(_iter, new TreeNodeString(0, 0, i.ToString()));
				}

				if( i is JObject )
				{
					aEnv.SetVar(_iter, new InnerObject(i));
				}

				foreach(var c in _commands)
				{
					c.Env = aEnv;
					var tmp = c.Value;
				}
			}
			aEnv.Pop();
			return null;
		}
		
	}
}
