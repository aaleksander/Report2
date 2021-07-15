using System;

namespace Editor.ReportDSL
{
	public class Page: XamlOperatorBase
	{
		public Page()
		{
			_formatOperations = new String[]{
				":margin", ":text-align", ":font-family", ":font-size", ":page-padding",
				":font-weight", ":page-width", ":page-height"
			};

			XamlOperator = "FlowDocument";
			Default =  "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
    			"xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +  
    			"ColumnWidth=\"99999\" ";
		}
	}
}
