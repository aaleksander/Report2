using System;

namespace Editor.ReportDSL
{


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
	
	public class Baseline: XamlOperatorBase
	{
		public Baseline(): base()
		{			
			_formatOperations = new String[]{":font-size"};
			XamlOperator = "Span";
			Default = "BaselineAlignment=\"Baseline\" ";
		}
	}

	public class Superscript: XamlOperatorBase
	{
		public Superscript(): base()
		{			
			_formatOperations = new String[]{":font-size"};
			XamlOperator = "Span";
			Default = "BaselineAlignment=\"Superscript\" ";
		}
	}
	
	public class Subscript: XamlOperatorBase
	{
		public Subscript(): base()
		{			
			_formatOperations = new String[]{":font-size"};
			XamlOperator = "Span";
			Default = "BaselineAlignment=\"Subscript\" ";
		}
	}
}
