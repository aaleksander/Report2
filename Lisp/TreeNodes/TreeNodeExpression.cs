using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLisp.TreeNodes
{
	public class TreeNodeExpression: TreeNodeBase
	{
		public TreeNodeExpression(int aLine, int aCol, ref StateMachine st, Stack<TreeNodeBase> aStack)
			: base(aLine, aCol, ref st, aStack)
		{
			Alias = "Expression";
			Modifier = TreeNodeModifier.Create(this);//new StubModifier();
		}

		public override object Value {
			get {  
				if( Childs.Count == 0 )
				{
					throw new EpmtyExpressionException(1, 1);
				}

				var op = Env.GetOp( Childs[0].Source);
				object res = null;
				if( op != null )
				{
					var pars = new TreeNodeBase[Childs.Count - 1];
					int i=0;
					foreach(var a in Childs.Skip(1)) //оператор пропускаем
					{
						a.Env = Env;
						pars[i] = a;
						i++;
					}
					try
					{
						res = op.Eval(Line, Col, ref Env, pars);
						if( op.IsReturn == false )
						{
							return null;
						}
						return Modifier.Modify(res);
					}catch(Exception e)
					{
						if( !(e is BaseException) )
							throw new BaseException(Line, Col, "Системная ошибка:" + e.Message);
						else
							throw e;
					}
				}

				//если мы здесь, значит это может быть функция
				var func = Env.GetFunc(Childs[0].Source);
				if( func == null )					
					throw new BaseException(Childs[0].Line, Childs[0].Col, "нет такого идентификатора: " + Childs[0].Source);
				//загружаем параметры из окружения
				Env.Push();
				func.LoadParams(ref Env, Childs.Skip(1).ToList());
				//входим в функцию				

				res = func.Eval(ref Env); //выполняем функцию
				Env.Pop(); //выходим из функции
				return Modifier.Modify(res);
			}
		}

		public override string ToString(int aLevel)
		{
			StringBuilder sb = new StringBuilder();

			var ind = CreateIntend(aLevel); //отступ

			sb.AppendLine(ind + "Expression");

			if( Count > 0 )
			{
				bool f = true;
				foreach(var c in Childs)
				{
					if( f == false )
					{
						sb.AppendLine("");
					}
					sb.Append(c.ToString(aLevel + 1));
					f = false;
				}				
			}
			return sb.ToString();
		}
	}
}
