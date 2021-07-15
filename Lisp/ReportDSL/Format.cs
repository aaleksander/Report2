using System;
using System.Collections.Generic;
using Editor.ReportDSL;

//функция format: все, что касается работы с округлениями
namespace RLisp.ReportDSL
{
	#region параметры
	/// <summary>
	/// базовый класс
	/// </summary>
	internal class FormatParamBase
	{
		public FormatParamBase(object aVal)
		{
			Val = aVal;
		}

		public object Val{private set; get;}

		/// <summary>
		/// приоритет выполнения
		/// </summary>
		public virtual int Priority{get{return 0;}}

		public virtual object Step1(object val)
		{
			return val;
		}

		public virtual object Step2(object val)
		{
			return val;
		}
	}

	/// <summary>
	/// округление
	/// </summary>
	internal class FormatRound: FormatParamBase
	{	
		public FormatRound(object aVal):base(aVal){}
		
		public override int Priority {
			get { return 10; }
		}
		public override object Step1(object val)
		{
			return Math.Round((double) val);
		}
	}
	

	/// <summary>
	/// разделитель дробной части
	/// </summary>
	internal class FormatSep: FormatParamBase
	{	
		public FormatSep(object aVal):base(aVal){}
		
		public override int Priority {//самый младший приоритет
			get { return 15; }
		}
		public override object Step2(object val)
		{
			return val.ToString().Replace(".", Convert.ToString(Val)).Replace(",", Convert.ToString(Val));;
		}
	}
	
	/// <summary>
	/// округление
	/// </summary>
	internal class AfterZeroRound: FormatParamBase
	{	
		public override int Priority {
			get { return 5; }
		}
		public AfterZeroRound(object aVal):base(aVal){}

		public override object Step1(object val)
		{
			var v = Convert.ToInt32(Val);
			var tmp = Math.Pow(10, v);
			
			return Convert.ToDouble(val)*tmp;
		}

		public override object Step2(object val)
		{
			var v = Convert.ToInt32(Val);
			var tmp = Math.Pow(10, v);
			//return (Convert.ToDouble(val)/tmp).ToString();
			return PadZeroes((Convert.ToDouble(val)/tmp).ToString(), v);
		}

		private string PadZeroes(string aVal, int aNum)
		{
			string res = aVal.ToString();

			int s = res.IndexOf(".");
			if( s == -1 && aNum > 0 )//запятой нет, а разряды нужны
			{
			   	res = res + ".";
			   	for(int i=0; i<aNum; i++)
			   		res = res + "0";
			}

			if( s != -1 ) //запятая уже есть
			{
				if( s + aNum < res.Length ) //надо удалить лишние цифры
				{
					while( res.Length - s - 1 > aNum )
					{
						res = res.Substring(0, res.Length - 1);
					}
					if( aNum == 0 )
					{
						res = res.Replace(",", "");
					}
				}
				else
				{//добавить нули
					while( res.Length - s <= aNum )
					{
						res += "0";
					}
				}
			}
			
			return res;
		}
		
	}
	#endregion

	[Description("Округлять")]
	public class Round:XamlParameter
	{
		public Round():base(){}
		
		public override bool IsReturn{get{ return true; }}

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params RLisp.TreeNodes.TreeNodeBase[] aPars)
		{
			return new FormatRound(null);
		}
	}

	[Description("Отделитель дробной части")]
	public class Sep:XamlParameter
	{
		public Sep():base(){}
		
		public override bool IsReturn{get{ return true; }}

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params RLisp.TreeNodes.TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Параметр :after-zero может содержать только один параметр");

			return new FormatSep(aPars[0].Value);
		}
	}

	[Description("Сколько знаков после запятой")]
	public class After_Dot:XamlParameter
	{
		public After_Dot():base()
		{

		}

		public override bool IsReturn{get{ return true; }}

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params RLisp.TreeNodes.TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Параметр :after-zero может содержать только один параметр");

			return new AfterZeroRound(aPars[0].Value);
		}
	}

	[CanParamAttribute(
		typeof(Round),
		typeof(After_Dot),
		typeof(Sep),
		typeof(JsonOperator)
	)]
	[Description("Форматировать число")]
	public class Format: XamlOperatorBase
	{
		public override bool IsReturn{get{ return true;}}
		
		public Format(): base()
		{
//			_formatOperations = new String[]{":round", ":after-zero", ":separator"};
			XamlOperator = "";
		}

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params RLisp.TreeNodes.TreeNodeBase[] aPars)
		{
			var list = new List<FormatParamBase>();
			object val = null; 
			foreach(var p in aPars)
			{
				var tmp = p.Value;
				if( tmp is FormatParamBase) //это параметр форматирования
				{
					list.Add(p.Value as FormatParamBase);
				}
				else //это переменная, которую надо отформатировать
				{
					val = tmp;					
				}
			}

			if( val == null )
				throw new BaseException(aLine, aCol, "переменная для форматирования не найдена");

			//сортируем операторы по приоритету
			list.Sort(new Cmp());
			//первый проход
			foreach(var o in list)
			{
				val = o.Step1(val);
			}
			//второй проход
			foreach(var o in list)
			{
				val = o.Step2(val);
			}
			
//			Out(val.ToString());
			return val;
		}
		
		/// <summary>
		/// класс для сортировки параметров по приоритету
		/// </summary>
		private class Cmp: IComparer<FormatParamBase>
		{
			public int Compare(FormatParamBase a, FormatParamBase b)
			{
				return a.Priority.CompareTo(b.Priority);
			}
		}
	}
}
