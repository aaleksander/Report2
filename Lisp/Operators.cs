using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using RLisp.ReportDSL;
using RLisp.TreeNodes;

namespace RLisp
{
	public interface IOperator
	{
		object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars);
		bool IsReturn{get;}
		string Alias{get;} //название с точки зрения RLispа
	}

	/// <summary>
	/// Переменная аккумулятор, которая может накапливать значение
	/// </summary>
	public class Accumulator
	{
		private int? _accI = null;
		private double? _accD = null;
		public Accumulator()
		{
			
		}

		public object Add(object aVal)
		{
			return Value;
		}

		public object Value
		{
			get{
				return 0;
			}
		}
	}

	#region +
	public class AddOperator: IOperator
	{
		public AddOperator(){}

		public string Alias{get{return "+";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			//TODO !!! сделать, чтобы работало с double
			int res = 0;
			object tmp;
			foreach(var p in aPars)
			{
				p.Env = aEnv;

				res += (int)p.Value;
			}

			return res;
		}

		public bool IsReturn{get{return true;}}

		public string Name{get{return "+";}}
	}
	#endregion

	#region -
	public class MinusOperator: IOperator
	{
		public MinusOperator(){}

		public string Name{get{return "-";}}

		public string Alias{get{return "-";}}
		
		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			int res = 0;
			if( aPars.Length < 2 )
				throw new BaseException(aLine, aCol, "Для вычитания нужно минимум два аргумента");

			res = (int)aPars[0].Value;
			foreach(var p in aPars.Skip(1))
			{
				p.Env = aEnv;
				res -= (int)p.Value;
			}			
			return res;
		}
		
		public bool IsReturn{get{return true;}}
	}
	#endregion

	#region *
	public class MulOperator: IOperator
	{
		public MulOperator(){}
		public bool IsReturn{get{return true;}}

		public string Alias{get{return "*";}}
		
		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			int res = 1;
			foreach(var p in aPars)
			{
				p.Env = aEnv;
				res *= (int)p.Value;
			}			
			return res;
		}
	}
	#endregion

	#region concat
	public class ConcatOperator: IOperator
	{
		public ConcatOperator(){}
		
		public string Alias{get{return "concat";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			StringBuilder res = new StringBuilder();

			foreach(var p in aPars)
			{
				p.Env = aEnv;
				res.Append( p.Value.ToString() );
			}			
			return res;
		}
		
		public bool IsReturn{get{return true;}}
	}
	#endregion

	#region Console
	public class ConsoleOperator: IOperator
	{
		public bool IsReturn{get{return false;}}
		
		public string Alias{get{return "?";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			object res = null;
			foreach(var s in aPars)
			{
				s.Env = aEnv;
				res = s.Value;
				if( res != null ) //всякие служебные штуки, типа (new-line) так делают
				{
					Environment.Console = res.ToString();//(s as TreeNodeString).Source;
				}
			}
			return null;
		}
	}
	#endregion

	#region def
	[CanParamAttribute(
		typeof(LoopOperator),
		typeof(DefOperator),
		typeof(AddOperator),
		typeof(MinusOperator),
		typeof(MulOperator),
		typeof(LoopOperator)
	)]
	public class DefOperator: IOperator
	{
		public bool IsReturn{get{return false;}}
		
		public string Alias{get{return "def";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars[0] is TreeNodeIdent )
			{//это простая переменная
				var name = aPars[0].ToString();// as TreeNodeIdent).Value.ToString();
				aEnv.SetVar(name, aPars[1]);// as TreeNodeBase;
				return null;
			}

			if( aPars[0] is TreeNodeExpression && aPars[1] is TreeNodeExpression )
			{//это новая функция
				Debug.WriteLine("Определяем новую функцию");
				aEnv.SetFunc(aPars[0] as TreeNodeExpression, 
				             aPars.Skip(1).Cast<TreeNodeExpression>()
				             	.ToArray());//[1] as TreeNodeExpression);
				return null;
			}

			throw new BaseException(aLine, aCol, "Непонятные параметры");
		}
	}
	#endregion

	/// <summary>
	/// общий класс для всех условий
	/// </summary>
	public class CondBase: IOperator
	{
		public virtual bool IsReturn{get{return true;}}

		public virtual string Alias{get{return "";}}
		
		public virtual object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			throw new BaseException(aLine, aCol, "нельзя вызывать базовый класс условия");
		}

		protected bool IsTrue(int aLine, int aCol, object aVal)
		{
			if( aVal is int )
				return (int)aVal != 0;

			if( aVal is string )
				return (string)aVal != "";

			if( aVal is TreeNodeIdent )
			{
				return IsTrue(aLine, aCol, (aVal as TreeNodeIdent).Value);
			}
			throw new BaseException(aLine, aCol, "Я не зная как интерпретировать в bool " + aVal.ToString());
		}
	}

	#region if

	public class ifOperator: CondBase
	{//оператор if, только без then
//		public override bool IsReturn{get{return false;}}

		public override string Alias{get{return "if";}}		

		public override object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length < 2 )
				throw new BaseException(aLine, aCol, "Оператор if должен иметь два или три аргумента");

			object res = null;
			object tmp = null;
			if( IsTrue(aLine, aCol, aPars[0].Value) )
			{
				//выполняем, пока не встретим "esle"
				foreach(var par in aPars.Skip(1))
				{
					if( par.Source == "else" )
					{
						break;
					}
					tmp = par.Value;
					if( tmp != null )
					{
						res = tmp;
					}
				}
					
				return res;
			}
			else //условие не истино
			{
				//ищем "esle"
				bool f = false;
				foreach(var par in aPars.Skip(1))
				{
					if( par.Source == "else" )
					{
						f = true;
						continue;
					}
					if( f == true )
					{//если мы тут, значит встретили else
						tmp = par.Value;
						if( tmp != null )
						{
							res = tmp;
						}
					}
				}
				return res;
			}
		}
	}
	#endregion

	#region json
	public class JsonOperator: IOperator
	{
		public bool IsReturn{get{return true;}}
		public string Alias{get{return "json";}}

		public virtual object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length < 1 )
				throw new BaseException(aLine, aCol, "Недостаточно параметров у оператора json");

			string path = aPars[0].Source;
			while( path.Contains("$") )
			{
				int i = path.IndexOf('$');
				string varName = path.Substring(i + 1, 1);
				var val = aEnv.GetVar(0, 0, varName); //прокинуть сюда Line и Col
				
				path = path.Replace("$" + varName, val.Value.ToString());
			}

			var a = aEnv.Json.SelectToken(path);

			if( a == null )
				throw new BaseException(aPars[0].Line, aPars[0].Col, "путь в json не найден");
			
			switch(a.Type)
			{
				case JTokenType.Object:
					return a.ToObject<object>();
				case JTokenType.Float:
					return a.ToObject<double>();
				case JTokenType.Integer: 
					return a.ToObject<int>();
				case JTokenType.Array: 
					return a.ToObject<object[]>();
				case JTokenType.String:
					return a.ToObject<string>();
			}

			throw new BaseException(aLine, aCol, "неизвестный тип переменной в json (" + a.Type.ToString() + ")");
		}
	}
	#endregion

	#region list
	public class ListOperator: IOperator
	{
		public bool IsReturn{get{return true;}}
		
		public string Alias{get{return "list";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			var res = new object[aPars.Length];
			int i=0;
			foreach(var p in aPars)
			{
				p.Env = aEnv;
				res[i] = p.Value;
				i++;
			}

			return res;
		}
	}
	#endregion

	#region new-line
	public class NewLineOperator: IOperator
	{
		public bool IsReturn{get{return false;}}
		
		public string Alias{get{return "new-line";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			RLisp.Environment.Console = System.Environment.NewLine;
			return null;
		}
	}
	#endregion

	#region сравнения

	public class ComparerBase: IOperator
	{
		public ComparerBase(Func<int, int, object, object, bool> aFunc)
		{
			_cmpFunc = aFunc;
		}
		
		public virtual string Alias{get{return "";}}

		public bool IsReturn{get{return true;}}

		protected static int Cmp(int aLine, int aCol, object a, object b)
		{
			if( IsEqual(aLine, aCol, a, b) )
			{
				return 0;
			}

			if( IsGreat(aLine, aCol, a, b) )
			{
				return 1;
			}

			if( IsLess(aLine, aCol, a, b) )
			{
				return -1;
			}
			throw new BaseException(aLine, aCol, "Результат сравнения непонятен");
		}

		public virtual object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length <= 1 )
				throw new BaseException(aLine, aCol, "Недостаточно параметров у оператора '='");

			//расчитываем значение первого параметра
			var first = aPars[0];
			first.Env = aEnv;
			var val = first.Value;

			//можно проверить, чтобы все аргументы были равны
			foreach(var p in aPars.Skip(1))
			{
				p.Env = aEnv;
				if( _cmpFunc(aLine, aCol, val, p.Value) == false ) //не подходит
					return 0;
			}			
			return 1; //все равны
		}

		private Func<int, int, object, object, bool> _cmpFunc = null;

		private static bool IsEqual(int aLine, int aCol, object a, object b)
		{
			if( a is int )
			{
				return (int)a == (int)b;
			}

			throw new BaseException(aLine, aCol, "Я не знаю как сравнивать " 
			                        + a.GetType().ToString() + " и " + b.GetType().ToString());
		}

		private static bool IsGreat(int aLine, int aCol, object a, object b)
		{
			if( a is int )
			{
				return (int)a > (int)b;
			}
			
			throw new BaseException(aLine, aCol, "Я не знаю как сравнивать " + a.ToString() + " и " + b.ToString());
		}

		private static bool IsLess(int aLine, int aCol, object a, object b)
		{
			if( a is int )
			{
				return (int)a < (int)b;
			}
			
			throw new BaseException(aLine, aCol, "Я не знаю как сравнивать " + a.ToString() + " и " + b.ToString());
		}
	}

	public class EqualOperator: ComparerBase
	{//сравнивает первый параметр со всеми остальными (произвольное количество пароматров
		public EqualOperator()
		                 	:base(
			(l, c, x, y) => Cmp(l, c, x, y) == 0
		)
		{}
		
		public override string Alias{get{return "=";}}
	}

	public class GreatOperator: ComparerBase
	{//сравнивает первый параметр со всеми остальными (произвольное количество пароматров
		public GreatOperator()
		                 	:base(
			(l, c, x, y) => Cmp(l, c, x, y) == 1
		)
		{}	
		
		public override string Alias{get{return ">";}}
	}
	
	public class GreatEqualOperator: ComparerBase
	{//сравнивает первый параметр со всеми остальными (произвольное количество пароматров
		public GreatEqualOperator()
		                 	:base(
			(l, c, x, y) => Cmp(l, c, x, y) >= 0
		)
		{}	
		
		public override string Alias{get{return ">=";}}
	}
	
	public class LessOperator: ComparerBase
	{//сравнивает первый параметр со всеми остальными (произвольное количество пароматров
		public LessOperator()
		                 	:base(
			(l, c, x, y) => Cmp(l, c, x, y) == -1
		)
		{}
		
		public override string Alias{get{return "<";}}
	}
	
	public class LessEqualOperator: ComparerBase
	{//сравнивает первый параметр со всеми остальными (произвольное количество пароматров
		public LessEqualOperator()
		                 	:base(
			(l, c, x, y) => Cmp(l, c, x, y) <= 0
		)
		{}
		
		public override string Alias{get{return "<=";}}
	}
	
	public class NotEqualOperator: ComparerBase
	{//сравнивает первый параметр со всеми остальными (произвольное количество пароматров
		public NotEqualOperator()
		                 	:base(
			(l, c, x, y) => Cmp(l, c, x, y) != 0
		)
		{}
		
		public override string Alias{get{return "!=";}}
	}
	#endregion

	#region or and not
	public class OrOperator: IOperator
	{
		public OrOperator(){}
		public string Alias{get{return "or";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			foreach(var p in aPars)
			{
				p.Env = aEnv;
				if( (int)p.Value != 0 )
					return 1;
			}			
			return 0;
		}

		public bool IsReturn{get{return true;}}
	}

	public class AndOperator: IOperator
	{
		public AndOperator(){}
		public string Alias{get{return "and";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			foreach(var p in aPars)
			{
				p.Env = aEnv;
				if( (int)p.Value != 1 )
					return 0;
			}			
			return 1;
		}

		public bool IsReturn{get{return true;}}
	}

	public class NotOperator: IOperator
	{
		public NotOperator(){}
		public string Alias{get{return "not";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length > 1 )
			{
				throw new BaseException(aLine, aCol, "У оператора not должен быть только один параметр: ");
			}

			aPars[0].Env = aEnv;
			if( (int)aPars[0].Value == 0 )
				return 1;
			else
				return 0;
		}

		public bool IsReturn{get{return true;}}
	}
	#endregion
	
	#region set!	
	/// <summary>
	/// назначить переменной значение
	/// </summary>
	public class SetOperator: IOperator
	{
		public bool IsReturn{get{return false;}}

		public string Alias{get{return "set!";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 2)
				throw new BaseException(aLine, aCol, "set! может иметь только два параметра");

			if( !(aPars[0] is TreeNodeIdent) )
				throw new BaseException(aPars[0].Line, aPars[0].Col, "Первым параметром set! должна быть переменная");

			var v = aEnv.GetVar(aLine, aCol, aPars[0].Source);
			aEnv.SetVar(aLine, aCol, aPars[0].Source, aPars[1].Value);
			return null;
//			aEnv.SetVar(aPars[0].Source, new TreeNodeBase(aLine, aCol, aPars[1].Value.ToString()));

			throw new BaseException(aLine, aCol, "Непонятные параметры");
		}
	}
	#endregion

	#region UpDown
	[Description("Строка с меняющимся индексом (то верхним, то нижним)")]
	public class UpDownOperator: IOperator
	{
		public bool IsReturn{get{return true;}}

		public string Alias{get{return "up-down";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 4 )
				throw new BaseException(aLine, aCol, "Неправильное количество параметров, нужно так: (up-down шрифтосновной шрифтверхний шрифтнижний шаблон)");

			int normal = 0;
			try{
				normal = int.Parse(aPars[0].Value.ToString());
			}catch(Exception)
			{
				throw new BaseException(aPars[0].Line, aPars[0].Col, "неправильный параметр '" + aPars[0].Source + "', должно быть целое число");
			}

			int sub = 0;
			try{
				sub = int.Parse(aPars[1].Value.ToString());
			}catch(Exception)
			{
				throw new BaseException(aPars[1].Line, aPars[1].Col, "неправильный параметр '" + aPars[1].Source + "', должно быть целое число");
			}
			
			int super = 0;
			try{
				super = int.Parse(aPars[2].Value.ToString());
			}catch(Exception)
			{
				throw new BaseException(aPars[2].Line, aPars[2].Col, "неправильный параметр '" + aPars[2].Source + "', должно быть целое число");
			}
			
			
         	Func<StringBuilder, String, int, StringBuilder> run = (a, b, m) => 
         	{
         		if( b.Length > 0 )
         		{
	         		switch(m)
	         		{
	         			case 0:
	         				Environment.Console = "<Run FontSize=\"" + normal.ToString() + "\" BaselineAlignment=\"Baseline\">";
	         				break;
	         			case -1:
	         				Environment.Console = "<Run FontSize=\"" + sub.ToString() + "\" BaselineAlignment=\"Subscript\">";
	         				break;
	         			case 1:
	         				Environment.Console = "<Run FontSize=\"" + super + "\" BaselineAlignment=\"Superscript\">";
	         				break;
	         		}
					Environment.Console = b;
					Environment.Console = "</Run>";
         		}
         		return a;
         	};
         	int mode = 0; //0-baseline, 1-superscript, -1-subscript
         	StringBuilder sb = new StringBuilder();
         	string s = "";

         	foreach(var ch in aPars[3].Value.ToString())
         	{
         		switch(ch)
         		{
         			case '_': 
							run(sb, s, mode);
     					s = "";
     					mode--;
         				break;
         			case '^': 
							run(sb, s, mode);
     					s = "";
     					mode++;
         				break;
         			default:
     					s = s + ch;
         				break;
         		}
         	}
			run(sb, s, mode);
         	return sb.ToString();
		}
	}
	#endregion	
	
	#region round
	public class RoundOperator: IOperator
	{
		public RoundOperator(){}

		public string Alias{get{return "round";}}

		public object Eval(int aLine, int aCol, ref Environment aEnv, params TreeNodeBase[] aPars)
		{
			return 0;
		}

		public bool IsReturn{get{return true;}}
	}
	#endregion
}
