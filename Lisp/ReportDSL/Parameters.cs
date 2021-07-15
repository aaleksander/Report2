using System;
using RLisp;
using RLisp.ReportDSL;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Параметр xaml-операторов (выравнивания, шрифты и прочее)
	/// </summary>
	public class XamlParameter: XamlOperatorBase
	{
		public string Unit{protected set; get;}

		public XamlParameter(): base()
		{
			//_formatOperations = new string[0];
			Prefix = ":";
			Unit = "";
		}

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aLine, aCol, aEnv, aPars);
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

	[Description("отступы")]
	public class Margin: XamlParameter
	{
		public Margin():base()
		{
			XamlOperator = "Margin";
		}
	}

	[Description("выравнивание текста")]
	public class Text_Align: XamlParameter
	{
		public Text_Align():base()
		{
			XamlOperator = "TextAlignment";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров TextAlignment = 1");
			string tmp = aPars[0].Value.ToString();
			if( tmp != "Center" && tmp != "Left" && tmp != "Right" && tmp != "Justify" )
				throw new BaseException(aLine, aCol, tmp + " - недопустимый параметр для text-align");
			
			return true;
		}
	}
	
	

	[Description("толщина шрифта: normal или bold")]
	public class Font_Weight: XamlParameter
	{
		public Font_Weight():base()
		{
			XamlOperator = "FontWeight";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров FontWeight = 1");
			string tmp = aPars[0].Value.ToString();
			if( tmp != "N6ormal" && tmp != "Bold" )
				throw new BaseException(aLine, aCol, tmp + " - недопустимый параметр для font-weight");
			
			return true;
		}
	}

	[Description("высота строки")]
	public class Line_Height: XamlParameter
	{
		public Line_Height():base()
		{
			XamlOperator = "LineHeight";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Количество параметров line-height должно быть равно 1");

			var tmp = aPars[0].Value;
			if( !(tmp is int) )
				throw new BaseException(aLine, aCol, "Параметром line-height может быть только целое число");

			return true;
		}
	}
	
	[Description("Отступ красной строки")]
	public class Text_Indent: XamlParameter
	{
		public Text_Indent():base()
		{
			XamlOperator = "TextIndent";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров text-indent = 1");
			
			var tmp = aPars[0].Value;
			if( !(tmp is int) )
				throw new BaseException(aLine, aCol, "Параметром text-indent может быть только целое число");
			
			return true;
		}
	}

	[Description("Название шрифта")]
	public class Font_Family: XamlParameter
	{
		public Font_Family():base()
		{
			XamlOperator = "FontFamily";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Количество параметров font-family должно быть равно 1");
			
			var tmp = aPars[0].Value;
			if( !(tmp is string) )
				throw new BaseException(aLine, aCol, "Параметром font-family может быть только строка");

			return true;
		}
	}

	[Description("Размер шрифта")]
	public class Font_Size: XamlParameter
	{
		public Font_Size():base()
		{
			XamlOperator = "FontSize";
		}
		
		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Количество параметров font-family должно быть равно 1");
			
			var tmp = aPars[0].Value;
			if( !(tmp is int) )
				throw new BaseException(aLine, aCol, "Параметром font-size может быть только целое число");

			return true;
		}
		
	}

	[Description("Поля страницы")]
	public class Page_Padding: XamlParameter
	{
		public Page_Padding():base()
		{
			XamlOperator = "PagePadding";
			Unit = "cm";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 4 )
				throw new BaseException(aLine, aCol, "Количество параметров :page-padding должно быть равно 4");

			//TODO проверить параметры на положительность
//			foreach(var p in aPars)
//			{
//				if( ((double)p.Value) < 0 )
//					throw new BaseException(p.Line, p.Col, "Параметром page-padding может быть только целое число");
//			}

			return true;
		}
	}

	[Description("Ширина страницы")]
	public class Page_Width: XamlParameter
	{
		public Page_Width():base()
		{
			XamlOperator = "PageWidth";
			Unit = "cm";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Количество параметров :page-width должно быть равно 1");
			return true;
		}
	}

	[Description("Высота страницы")]
	public class Page_Height: XamlParameter
	{
		public Page_Height():base()
		{
			XamlOperator = "PageHeight";
			Unit = "cm";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Количество параметров :page-height должно быть равно 1");
			return true;
		}
	}
}
