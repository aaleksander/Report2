using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RLisp;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Параграф для xaml-а
	/// </summary>
	public class Par: XamlOperatorBase
	{
		public Par()
		{
			_formatOperations = new String[]{
				":margin", ":text-align", ":line-height", ":font-family", ":font-size",
				":font-weight"
			};

			XamlOperator = "Paragraph";
		}
	}

	public class Par_C: Par
	{
		public Par_C(): base()
		{
			Default = @"TextAlignment=""Center"" ";
			
			_formatOperations = _formatOperations.Where(x => x != ":text-align").ToArray();
		}
	}

	public class Par_L: Par_C
	{
		public Par_L(): base()
		{
			Default = @"TextAlignment=""Left"" ";
		}
	}

	public class Par_R: Par_C
	{
		public Par_R(): base()
		{
			Default = @"TextAlignment=""Right"" ";
		}
	}

	public class Par_J: Par_C
	{
		public Par_J(): base()
		{
			Default = @"TextAlignment=""Justify"" ";
		}
	}
}
