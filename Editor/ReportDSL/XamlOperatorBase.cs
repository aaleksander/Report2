using System;
using RLisp;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Базовый класс для всех xaml-операторов
	/// </summary>
	public class XamlOperatorBase:IOperator
	{
		public bool IsReturn{get{return false;}}
		public XamlOperatorBase()
		{
			_formatOperations = new String[0];
			Prefix = "";
		}

		/// <summary>
		/// строка - оператор, которая вставляется в xaml-код
		/// </summary>
		public string XamlOperator{protected set; get;}
		
		/// <summary>
		/// название оператора с точки зрения RLisp
		/// </summary>
		public string Name{set; get;}

		public string Prefix{protected set; get;} //тут либо двоеточие, либо пусто
		
		/// <summary>
		/// то, что добавляют после Alias по умолчанию
		/// </summary>
		public string Default{protected set; get;}

		/// <summary>
		/// проверяет правильность параметров
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( _formatOperations == null )
			{
				throw new Exception("операции форматирования не определены");
			}

			//проверим, чтобы передали только то, что допускается в _formatOperations
			foreach(var p in aPars)
			{
				if( p.Childs != null && p.Count > 0 )
				{
					if( p.Childs[0].Source.StartsWith(":") )
					{
						if( AllowFormat(p.Childs[0].Source) == false )
						{
							throw new Exception("Параметр " + p.Childs[0].Source + " не предусмотрен для " + Name);
						}
					}
				}
			}

			return true;
		}

		public virtual object Eval(ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aPars);

			Out("<" + XamlOperator + " " + Default);

			//печатает форматирование параграфа
			foreach(var p in aPars)
			{
				if( p.Count == 0 ) //это какай-то константы, потом их напечатаем
					continue;
				if( AllowFormat(p.Childs[0].Source) )
				{
					//(p as TreeNodeExpression).Value;
					var t = p.Value;
				}
			}

			Out(">");

			//отбираем строки
			foreach(var s in aPars)
			{
				if( s.Count == 0 )
				{
					Out(s.Value.ToString());
					continue;
				}
				if( AllowFormat(s.Childs[0].Source) == true ) //это форматирование
					continue;
				//тут остались какие-то выражения
				var t = s.Value;	
				if( t != null )
				{
					Out(t.ToString());
				}
			}

			Out("</" + XamlOperator + ">"); //закрываем
			return null;
		}

		protected void Out(string aS)
		{
			RLisp.Environment.Console = aS;
		}

		/// <summary>
		/// список операторов, которые могут быть использованы в форматировании
		/// </summary>
		protected string[] _formatOperations = null;

		protected bool AllowFormat(string aName)
		{
			foreach(var f in _formatOperations)
			{
				if( f == aName )
					return true;
			}
			return false;
		}
	}
}
