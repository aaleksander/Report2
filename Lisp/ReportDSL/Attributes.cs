using System;

namespace RLisp.ReportDSL
{
	/// <summary>
	/// атрибут задает типы "детей", которые может иметь XamlOperator в качестве xaml-параметров
	/// например <Paragraph TextAlign, Margin>test</Paragraph>  TextAlign и Margin - это оно, test - нет  
	/// </summary>
	public class CanParamAttribute: Attribute
	{
		public Type[] List{set;get;}
		public CanParamAttribute(params Type[] aTypes)
		{
			List = aTypes;
		}
	}

	//TODO нужен еще один тип атрибутов: которые не являются форматированием
	public class CanChildAttribute: Attribute
	{
		public Type[] List{set;get;}
		public CanChildAttribute(params Type[] aTypes)
		{
			List = aTypes;
		}
	}

	/// <summary>
	/// текстовое описание
	/// </summary>
	public class DescriptionAttribute: Attribute
	{
		public string Text{private set; get;}
		
		public DescriptionAttribute(string aText)
		{
			Text = aText;
		}
	}
}
