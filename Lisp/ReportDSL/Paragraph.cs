using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RLisp;
using RLisp.ReportDSL;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Параграф для xaml-а
	/// </summary>
	[CanParamAttribute(
		typeof(Margin),
	    typeof(Text_Align), 
	    typeof(Line_Height),
	    typeof(Font_Family),
	    typeof(Font_Size),
	    typeof(Font_Family),
	    typeof(Font_Weight),
	    typeof(Format),
	    typeof(Text_Indent)
	   )]
	[Description("Абзац")]
	public class Par: XamlOperatorBase
	{
		public Par(): base()
		{
			XamlOperator = "Paragraph";
		}
	}

	[CanParamAttribute(
			typeof(Margin),
	        typeof(Line_Height),
	        typeof(Font_Family),
	        typeof(Font_Size),
	        typeof(Font_Family),
	        typeof(Font_Weight)
	       )]
	[Description("Абзац, выравнивание по центру")]
	public class Par_C: Par
	{
		public Par_C(): base()
		{
			Default = @"TextAlignment=""Center"" ";
			
//			_formatOperations = _formatOperations.Where(x => x != ":text-align").ToArray();
		}
	}
	
	[CanParamAttribute(
			typeof(Margin),
	        typeof(Line_Height),
	        typeof(Font_Family),
	        typeof(Font_Size),
	        typeof(Font_Family),
	        typeof(Font_Weight)
	       )]
	[Description("Абзац, выравнивание по левому краю")]
	public class Par_L: Par_C
	{
		public Par_L(): base()
		{
			Default = @"TextAlignment=""Left"" ";
		}
	}
	
	[CanParamAttribute(
			typeof(Margin),
	        typeof(Line_Height),
	        typeof(Font_Family),
	        typeof(Font_Size),
	        typeof(Font_Family),
	        typeof(Font_Weight)
	       )]
	[Description("Абзац, выравнивание по правому краю")]
	public class Par_R: Par_C
	{
		public Par_R(): base()
		{
			Default = @"TextAlignment=""Right"" ";
		}
	}
	
	[CanParamAttribute(
			typeof(Margin),
	        typeof(Line_Height),
	        typeof(Font_Family),
	        typeof(Font_Size),
	        typeof(Font_Family),
	        typeof(Font_Weight)
	       )]
	[Description("Абзац, выравнивание по ширине")]
	public class Par_J: Par_C
	{
		public Par_J(): base()
		{
			Default = @"TextAlignment=""Justify"" ";
		}
	}
}
