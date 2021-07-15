using System;
using System.Collections.Generic;
using RLisp;
using RLisp.ReportDSL;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Базовый класс для всех xaml-операторов
	/// </summary>
	public class XamlOperatorBase:IOperator
	{
		public static Type Type{get{return typeof(XamlOperatorBase);}}
		
		public string Alias{get{return Name;}}
		public virtual bool IsReturn{get{return false;}}
		public XamlOperatorBase()
		{
			Prefix = "";

			//выясняем атрибуты
			Type t = this.GetType();
			var props = (CanParamAttribute)Attribute.GetCustomAttribute(t, typeof(CanParamAttribute));
			if( props != null )
				_formatOperations = props.List;
			else
				_formatOperations = new Type[0];
		}

		/// <summary>
		/// строка - оператор, которая вставляется в xaml-код
		/// </summary>
		public string XamlOperator{protected set; get;}

		/// <summary>
		/// название оператора с точки зрения RLisp
		/// </summary>
		public string Name{
			get
			{
				var tmp = this.GetType().Name.ToLower();
				tmp = tmp.Replace('_', '-');
				tmp = Prefix + tmp;
				return tmp;
			}
		}

		public string Prefix{protected set; get;} //тут либо двоеточие, либо пусто

		/// <summary>
		/// то, что добавляют после Alias по умолчанию
		/// </summary>
		public string Default{protected set; get;}

		/// <summary>
		/// проверяет правильность параметров
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( _formatOperations == null )
			{
				throw new BaseException(aLine, aCol, "операции форматирования не определены");
			}

			//проверим, чтобы передали только то, что допускается в _formatOperations
			foreach(var p in aPars)
			{
				if( p.Childs != null && p.Count > 0 )
				{
					if( p.Childs[0].Source.StartsWith(":") )
					{
                        //var ttt = p.Value;
                        
						if( AllowFormat(aEnv, p.Childs[0].Source) == false )
						{
							throw new BaseException(aLine, aCol, "Параметр " + p.Childs[0].Source + " не предусмотрен для " + Name);
						}
					}
				}
			}

			return true;
		}

		public virtual object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aLine, aCol, aEnv, aPars);

			Out("<" + XamlOperator + " " + Default);

			//печатает форматирование
			foreach(var p in aPars)
			{
				if( p.Count == 0 ) //это какай-то константы, потом их напечатаем
					continue;
				if( AllowFormat(aEnv, p.Childs[0].Source) )
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
				if( AllowFormat(aEnv, s.Childs[0].Source) == true ) //это форматирование
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
		protected Type[] _formatOperations = null;

		protected bool AllowFormat(RLisp.Environment aEnv, string aName)
		{
			var op = aEnv.GetOp(aName);
			if( op == null )
				return false;
			foreach(var f in _formatOperations)
			{
				if( f == op.GetType() )
					return true;
			}
			return false;
		}
	}
}
