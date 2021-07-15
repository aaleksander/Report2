using System;
using RLisp.ReportDSL;

namespace Editor.ReportDSL
{
//форматирование строк

	public class Underline: XamlOperatorBase
	{
		public Underline(): base()
		{			
			XamlOperator = "Underline";
		}
	}

	public class Save_space: XamlOperatorBase
	{
		public Save_space(): base()
		{			
			XamlOperator = "Run";
			Default = "xml:space=\"preserve\"";
		}
	}

	[CanParamAttribute(typeof(Font_Size))]
	public class Baseline: XamlOperatorBase
	{
		public Baseline(): base()
		{
//			_formatOperations = new String[]{":font-size"};
			XamlOperator = "Span";
			Default = "BaselineAlignment=\"Baseline\" ";
		}
	}

	[CanParamAttribute(typeof(Font_Size))]
	public class Superscript: XamlOperatorBase
	{
		public Superscript(): base()
		{			
//			_formatOperations = new String[]{":font-size"};
			XamlOperator = "Span";
			Default = "BaselineAlignment=\"Superscript\" ";
		}
	}

	[CanParamAttribute(typeof(Font_Size))]
	public class Subscript: XamlOperatorBase
	{
		public Subscript(): base()
		{			
			//_formatOperations = new String[]{":font-size"};
			XamlOperator = "Span";
			Default = "BaselineAlignment=\"Subscript\" ";
		}
	}
	

}
