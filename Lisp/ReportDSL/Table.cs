using System;
using RLisp;
using RLisp.ReportDSL;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Таблица в репорте
	/// </summary>
	[CanParamAttribute(
		typeof(Cell_Spacing),
        typeof(Columns),
        typeof(Margin)
//        typeof(Row),
//        typeof(LoopOperator)
       )]
	[Description("таблица")]
	public class Table: XamlOperatorBase
	{
		public Table(): base()
		{
//			_formatOperations = new String[]{
//				":cell-spacing", ":columns", ":margin"
//			};
			XamlOperator = "Table";
		}

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aLine, aCol, aEnv, aPars);

			Out("<Table ");

			//печатает заголовк таблицы
			foreach(var p in aPars)
			{
				if( AllowFormat(aEnv, p.Childs[0].Source) && p.Childs[0].Source != ":columns" )
				{
					var t = p.Value;
				}
			}
			Out(">");

			//ищем и печатает столбцы
			bool okCol = false; //признак того, что описание столбцов нашли
			foreach(var p in aPars)
			{
				if( AllowFormat(aEnv, p.Childs[0].Source) && p.Childs[0].Source == ":columns" )
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
					if( AllowFormat(aEnv, s.Childs[0].Source) == true ) //это форматирование
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

//	[CanChild(
//		typeof(Cell)
//	)]
	[Description("строка таблицы")]
	public class Row: XamlOperatorBase
	{
		public Row(): base()
		{
			XamlOperator="TableRow";
		}
	}

	[CanParamAttribute(
		typeof(Border),
        typeof(Column_Span),
        typeof(Row_Span)
//        typeof(Par)
       )]
	[Description("ячейка таблицы")]
	public class Cell: XamlOperatorBase
	{
		public Cell(): base()
		{
			XamlOperator="TableCell";
//			_formatOperations = new String[]{
//				":border", ":column-span", ":row-span"
//			};
		}
	}

//вспомогательные параметры
	[Description("границы у ячейки (толщина линий)")]
	public class Border: XamlParameter
	{
		public Border():base()
		{
			XamlOperator = "BorderBrush=\"Black\" BorderThickness";
		}
	}

	[Description("объединение ячеек по горизонтали")]
	public class Column_Span: XamlParameter
	{
		public Column_Span():base()
		{
			XamlOperator = "ColumnSpan";
		}
	}

	[Description("объединение ячеек по вертикали")]
	public class Row_Span: XamlParameter
	{
		public Row_Span():base()
		{
			XamlOperator = "RowSpan";
		}
	}

	[Description("Список столбцов (параметры - ширины каждого столбца)")]
	public class Columns: XamlParameter
	{
		public Columns():base()
		{
			XamlOperator = "Columns";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
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

		public override object Eval(int aLine, int aCol, ref RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			CheckParams(aLine, aCol, aEnv, aPars);
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

	[Description("расстояние между ячейками")]
	public class Cell_Spacing: XamlParameter
	{
		public Cell_Spacing():base()
		{
			XamlOperator = "CellSpacing";
		}

		public override bool CheckParams(int aLine, int aCol, RLisp.Environment aEnv, params TreeNodeBase[] aPars)
		{
			if( aPars.Length != 1 )
				throw new BaseException(aLine, aCol, "Количество параметров :page-spacing должно быть равно 4");
			return true;
		}
	}
}