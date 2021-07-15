using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Editor.ReportDSL;
using Newtonsoft.Json.Linq;
using RLisp.TreeNodes;

namespace RLisp
{
	/// <summary>
	/// стэковое окружение для интерпретатора
	/// </summary>
	public class Environment
	{
		public Environment()
		{
			//TODO оператор деления "/"

			//добавляем в окружение все наши операторы
			Type ourType = typeof(IOperator);
			var list = Assembly.GetAssembly(ourType)
				.GetTypes()
				.OrderBy(x => x.Name);

			string tmp = "";
			foreach(var t in list)
			{
				tmp = t.Name.ToLower();				
				if( tmp.EndsWith("operator") && tmp != "ioperator" )
				{
					var obj = Activator.CreateInstance(t) as IOperator;	
					if( obj.Alias == "" )
						throw new Exception("Не назначен Alias классу оператора " + t.Name);
					_opers[obj.Alias] = obj;
				}
			}

			Push();
		}

		#region консоль (для - всех общая)
		private static StringBuilder _console = new StringBuilder();

		public static string Console{
			set{
				_console.Append(value);
			}
			get{
				return _console.ToString();
			}
		}

		public static void NewLine()
		{
			_console.Append(System.Environment.NewLine);
		}
		
		public static void ConsoleClear()
		{
			_console.Clear();
		}
		#endregion

		#region переменные
		private Stack<Dictionary<string, TreeNodeBase>> Vars = new Stack<Dictionary<string, TreeNodeBase>>();

		public void SetVar(string aName, TreeNodeBase aVar)
		{
			Vars.Peek()[aName] = aVar;
		}

		/// <summary>
		/// присваиваем переменной конкретное значение
		/// </summary>
		/// <param name="aLine"></param>
		/// <param name="aCol"></param>
		/// <param name="aName"></param>
		/// <param name="aVar"></param>
		public void SetVar(int aLine, int aCol, string aName, object aVar)
		{
			var arr = Vars.ToArray();
			TreeNodeBase ident = null;
			
			foreach(var l in arr)
			{
				if( l.ContainsKey(aName) )
				{
					ident = l[aName];
					break;
				}
			}
			
			if( ident == null )
				throw new BaseException(aLine, aCol, "Переменная " + aName + " не найдена");

//			var ident = Vars.Peek()[aName];

			if( ident is TreeNodeNumber )
			{
				(ident as TreeNodeNumber).SetSource(aLine, aCol, aVar.ToString());
				return;
			}

			if( ident is TreeNodeString )
			{
				(ident as TreeNodeString).SetSource(aLine, aCol, aVar.ToString());
				return;
			}

			throw new BaseException(aLine, aCol, "Нельзя присвоить значение типу " + ident.GetType().ToString());
		}

		public TreeNodeBase GetVar(int aLine, int aCol, string aName)
		{
			var arr = Vars.ToArray();

			var pars = aName.Split('.');

			foreach(var a in arr)
			{
				if( a.ContainsKey(pars[0]) )
				{
					var o = a[pars[0]];
					if( pars.Length == 1 )
					{//одиночный
						return o;
					}
					else
					{//составной путь в json
						if( o.Value is JObject )
						{
							StringBuilder sb = new StringBuilder();
							foreach(var p in pars.Skip(1))
							{
								if( sb.Length > 0 )
									sb.Append(".");
								sb.Append(p);
							}

							var res = (o.Value as JObject).SelectToken(sb.ToString());
							if( res == null )
								throw new BaseException(aLine, aCol, "переменная " + aName + " не определена");

							switch(res.Type)
							{
								case JTokenType.Integer: 
									return new TreeNodeNumber(0, 0, res.ToObject<int>().ToString());
								case JTokenType.String:
									return new TreeNodeString(0, 0, res.ToObject<string>());
								case JTokenType.Array:
									return new InnerObject(res.ToObject<object[]>());
								case JTokenType.Float:
									return new InnerObject(res.ToObject<double>());
							}
						}
					}
				}
			}

			throw new BaseException(aLine, aCol, "переменная " + pars[0] + " не определена");
		}
		#endregion

		#region Функции
		private Stack<Dictionary<string, Function>> Funcs = new Stack<Dictionary<string, Function>>();
		public void SetFunc(TreeNodeExpression aHeader, 
		                    params TreeNodeExpression[] aBody)
		{
			var newF = new Function(aHeader, aBody);
			Funcs.Peek()[newF.Name] = newF;
		}

		public Function GetFunc(string aName)
		{
			var arr = Funcs.ToArray();
			foreach(var a in arr)
			{
				if( a.ContainsKey(aName) )
				{//нашли функцию
					return a[aName];
				}
			}
			return null;
		}
		#endregion

		#region Операнды		
		public IOperator GetOp(string aAlias)
		{
			if( _opers.ContainsKey(aAlias ) )
			{
				return _opers[aAlias];
			}
			else
			{//ищем среди функций
				return null;
			}
		}
		protected Dictionary<string, IOperator> _opers= new Dictionary<string, IOperator>();
		#endregion

		public void Push()
		{
			Vars.Push(new Dictionary<string, TreeNodeBase>());
			Funcs.Push(new Dictionary<string, Function>());
		}

		public void Pop()
		{
			Vars.Pop();
			Funcs.Pop();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			var arr = Vars.ToArray();
			
			int level = 1;
			foreach(var a in arr)
			{
				sb.AppendLine("level: " + level.ToString());
				foreach(var v in a.Keys)
				{
					sb.Append(v.ToString() + "(" + a[v].Value + ") , ");
				}
				level++;
				sb.AppendLine("");
			}

			return sb.ToString();
			//return string.Format("[Environment Vars={0}, Funcs={1}, Opers={2}]", Vars, Funcs, _opers);
		}

		public JObject Json{set;get;}
	}
}
