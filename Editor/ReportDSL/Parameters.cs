using System;
using RLisp;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// базовый класс для всех xaml-операций
	/// </summary>
	public class XamlParameter: XamlOperatorBase
	{
		public string Unit{protected set; get;}

		public XamlParameter()
		{
			_formatOperations = new string[0];
			Prefix = ":";
			Unit = "";
		}

		public override object Eval(ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aPars);
			Out("" + XamlOperator + "=\"");
			bool f = false;
			foreach(var p in aPars)
			{
				if( f == true )
					Out(",");
				Out(p.Value.ToString());                    
				Out(Unit);
				f = true;
			}
			Out("\" ");
			return null;
		}
	}

	public class Margin: XamlParameter
	{
		public Margin():base()
		{
			XamlOperator = "Margin";
		}
	}

	public class Text_Align: XamlParameter
	{
		public Text_Align():base()
		{
			XamlOperator = "TextAlignment";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров TextAlignment = 1");
			string tmp = aPars[0].Value.ToString();
			if( tmp != "Center" && tmp != "Left" && tmp != "Right" && tmp != "Justify" )
				throw new Exception(tmp + " - недопустимый параметр для text-align");
			
			return true;
		}
	}
	
	public class Font_Weight: XamlParameter
	{
		public Font_Weight():base()
		{
			XamlOperator = "FontWeight";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров FontWeight = 1");
			string tmp = aPars[0].Value.ToString();
			if( tmp != "N6ormal" && tmp != "Bold" )
				throw new Exception(tmp + " - недопустимый параметр для font-weight");
			
			return true;
		}
	}

	public class Line_Height: XamlParameter
	{
		public Line_Height():base()
		{
			XamlOperator = "LineHeight";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров line-height должно быть равно 1");

			var tmp = aPars[0].Value;
			if( !(tmp is int) )
				throw new Exception("Параметром line-height может быть только целое число");

			return true;
		}
	}

	public class Font_Family: XamlParameter
	{
		public Font_Family():base()
		{
			XamlOperator = "FontFamily";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров font-family должно быть равно 1");
			
			var tmp = aPars[0].Value;
			if( !(tmp is string) )
				throw new Exception("Параметром font-family может быть только строка");

			return true;
		}
	}

	public class Font_Size: XamlParameter
	{
		public Font_Size():base()
		{
			XamlOperator = "FontSize";
		}
		
		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров font-family должно быть равно 1");
			
			var tmp = aPars[0].Value;
			if( !(tmp is int) )
				throw new Exception("Параметром font-size может быть только целое число");

			return true;
		}
		
	}

	public class Page_Padding: XamlParameter
	{
		public Page_Padding():base()
		{
			XamlOperator = "PagePadding";
			Unit = "cm";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 4 )
				throw new Exception("Количество параметров :page-padding должно быть равно 4");

			var tmp = aPars[0].Value;
			if( !(tmp is int) )
				throw new Exception("Параметром font-size может быть только целое число");

			return true;
		}
	}
	
	public class Page_Width: XamlParameter
	{
		public Page_Width():base()
		{
			XamlOperator = "PageWidth";
			Unit = "cm";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров :page-width должно быть равно 1");
			return true;
		}
	}

	public class Page_Height: XamlParameter
	{
		public Page_Height():base()
		{
			XamlOperator = "PageHeight";
			Unit = "cm";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров :page-height должно быть равно 1");
			return true;
		}
	}
}
