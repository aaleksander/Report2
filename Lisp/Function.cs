using System;
using System.Collections.Generic;
using System.Linq;
using RLisp.TreeNodes;

namespace RLisp
{
	/// <summary>
	/// Представление функции
	/// </summary>
	public class Function
	{
		public Function(TreeNodeExpression aHeader, 
		                params TreeNodeExpression[] aBody)
		{
			if( aHeader.Count == 0 )
				throw new Exception("У функции нету головы");

			Name = aHeader.Childs[0].Source;

			_body = aBody.ToList();;

			foreach(var p in aHeader.Childs.Skip(1))
			{
				_params.Add(Tuple.Create<string, TreeNodeBase>(p.Source, null));
			}
		}

		public string Name{private set; get;}
		private List<Tuple<string, TreeNodeBase>> _params = new List<Tuple<string, TreeNodeBase>>(); //список параметров
		private List<TreeNodeExpression> _body = null;
		private List<TreeNodeBase> _tail = null;

		public void LoadParams(ref Environment aEnv, List<TreeNodeBase> aParams)
		{
			int i=0; 
			_tail = new List<TreeNodeBase>();
			foreach(var p in aParams)
			{
				if( i < _params.Count ) //помещается в список параметров
				{
					p.Env = aEnv;
					aEnv.SetVar(_params[i].Item1, p);
				}
				else //это параметр лишний, мы засунем его как параметр в последнее выражение (или в ...)
				{
					_tail.Add(p);
				}
				i++;
			}
		}

		public TreeNodeExpression Find3Dots(List<TreeNodeExpression> aList)
		{//ищем в дереве
			TreeNodeExpression res = null;
			foreach(var i in aList)
			{
				//ищем, есть ли в списке потомков 3 точки
				res = (TreeNodeExpression) i.FindNode(x => x.Childs != null && x.Childs.Exists(a => a is TreeNodeDots)); 
				if( res != null )
				{
					break;
				}
			}
			return res;
		}

//TODO вокруг многоточия могут быть другие параметры (par "начало" ... "конец") - надо это протестить
		public object Eval(ref Environment aEnv)
		{
			object res = null, tmpRes;
			TreeNodeBase dd = null;
			int ind = 0;
			//ищем в теле выражение, которое принимает многоточие
			var exp = Find3Dots(_body);
			var flag = exp != null;

			if( exp == null ) //в данной ветке нет многоточий
			{ //просто пихаем все параметры последнему выражению
				exp = _body.Last();
				exp.Childs.AddRange(_tail);
			}
			else
			{//есть многоточние
				//забиваем многоточия параметрами
				dd = exp.Childs.First(x => x is TreeNodeDots);
				ind = exp.Childs.IndexOf(dd);
				var tmp = ind;
				exp.Childs.Remove(dd);	//удаляем многоточие, чтобы не мешалось
				foreach(var ee in _tail)
				{
					exp.Childs.Insert(tmp, ee);
					tmp++;
				}
			}

			foreach(var e in _body) //выполняем тело
			{
				e.Env = aEnv;
				tmpRes = e.Value;
				res = (tmpRes == null)? res: tmpRes;
			}
			//чистим Childs
			foreach(var ee in _tail)
			{
				exp.Childs.Remove(ee);
			}

			if( flag ) //вставляем многоточие обратно, если надо
			{
				exp.Childs.Insert(ind, dd);
			}
			return res;
		}		
	}
}
