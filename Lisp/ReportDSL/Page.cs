using System;
using RLisp;
using RLisp.ReportDSL;

namespace Editor.ReportDSL
{
	[CanParamAttribute(
		typeof(Margin),
		typeof(Text_Align),
		typeof(Font_Family),
		typeof(Font_Size),
		typeof(Font_Weight),
		typeof(Page_Padding),
		typeof(Page_Height),
		typeof(Page_Width)
	)]
	[CanChild(
		typeof(DefOperator),
		typeof(Par),
		typeof(Par_C),
		typeof(Par_L),
		typeof(Par_R),
		typeof(Par_J),
		typeof(Table)
	)]
	[Description("Страница отчета")]
	public class Page: XamlOperatorBase
	{
		public Page(): base()
		{
			XamlOperator = "FlowDocument";
			Default =  "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
    			"xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +  
    			"ColumnWidth=\"99999\" ";
		}
	}
}
