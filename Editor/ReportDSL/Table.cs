using System;
using RLisp;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Таблица в репорте
	/// </summary>
	public class Table: XamlOperatorBase
	{
		public Table(): base()
		{
			_formatOperations = new String[]{
				":cell-spacing", ":columns", ":margin"
			};
			XamlOperator = "Table";
		}

		public override object Eval(ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aPars);

			Out("<Table ");

			//печатает заголовк таблицы
			foreach(var p in aPars)
			{
				if( AllowFormat(p.Childs[0].Source) && p.Childs[0].Source != ":columns" )
				{
					var t = p.Value;
				}
			}
			Out(">");

			//ищем и печатает столбцы
			bool okCol = false; //признак того, что описание столбцов нашли
			foreach(var p in aPars)
			{
				if( AllowFormat(p.Childs[0].Source) && p.Childs[0].Source == ":columns" )
				{
					okCol = true;
					var tmp = p.Value;					
				}
			}	

			if( okCol == false )
				throw new Exception("Описание столбцов не обнаруженно");

			//отбираем строки
			Out("<TableRowGroup>");
			foreach(var s in aPars)
			{
					if( s.Count == 0 )
					{
						Out(s.Value.ToString());
						continue;
					}
					if( AllowFormat(s.Childs[0].Source) == true ) //это форматирование
						continue;
					//тут остались какие-то выражения
					var t = s.Value;	
					if( t != null )
					{
						Out(t.ToString());
					}
			}
			Out("</TableRowGroup>");

			Out("</" + XamlOperator + ">"); //закрываем
			return null;
			
		}
	}

	public class Row: XamlOperatorBase
	{
		public Row(): base()
		{
			XamlOperator="TableRow";
		}
	}

	public class Cell: XamlOperatorBase
	{
		public Cell(): base()
		{
			XamlOperator="TableCell";
			_formatOperations = new String[]{
				":border", ":column-span", ":row-span"
			};
		}
	}

	public class Cell_Center:XamlOperatorBase
	{
		public Cell_Center(): base()
		{

		}
	}

//вспомогательные параметры
	public class Border: XamlParameter
	{
		public Border():base()
		{
			XamlOperator = "BorderBrush=\"Black\" BorderThickness";
		}
	}

	public class Column_Span: XamlParameter
	{
		public Column_Span():base()
		{
			XamlOperator = "ColumnSpan";
		}
	}

	public class Row_Span: XamlParameter
	{
		public Row_Span():base()
		{
			XamlOperator = "RowSpan";
		}
	}

	public class Columns: XamlParameter
	{
		public Columns():base()
		{
			XamlOperator = "Columns";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			//проверить чтобы все параметры были положительными intами
			foreach(var c in aPars)
			{
				var val = c.Value;
				if( (val is int) == false )
					throw new BaseException(c.Line, c.Col, "Параметры :columns - только положительные целые числа!");
				if( (int)val < 0 )
					throw new BaseException(c.Line, c.Col, "Параметры :columns - только положительные целые числа!");
			}
			return true;
		}

		public override object Eval(ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aPars);
			Out("<Table.Columns>");
			//печатает форматирование параграфа
			foreach(var p in aPars)
			{
				Out("<TableColumn Width=\"" + p.Value + "\"/>");
			}
			Out("</Table.Columns>"); //закрываем
			return null;
		}
	}	

	public class Cell_Spacing: XamlParameter
	{
		public Cell_Spacing():base()
		{
			XamlOperator = "CellSpacing";
		}

		public override bool CheckParams(params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new Exception("Количество параметров :page-spacing должно быть равно 1");
			return true;
		}
	}
}