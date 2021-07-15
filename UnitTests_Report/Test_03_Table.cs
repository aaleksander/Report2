using System;
using NUnit.Framework;

namespace UnitTests_Report
{
	[TestFixture]
	public class Test_03_Table: Test_00
	{
		[Test]
		public void Test_01_()
		{
			var i = GetI();
			i.Eval("(table (:columns))");
			Assert.AreEqual(i.Console, "<Table ><Table.Columns></Table.Columns><TableRowGroup></TableRowGroup></Table>");
		}

		[Test]
		public void Test_02_CellSpacing()
		{
			var i = GetI();
			i.Eval("(table (:cell-spacing 3) (:columns))");
			Assert.AreEqual(i.Console, "<Table CellSpacing=\"3\" ><Table.Columns></Table.Columns><TableRowGroup></TableRowGroup></Table>");
		}

		[Test]
		public void Test_03_Columns()
		{
			var i = GetI();
			i.Eval("(table (:columns 12 13 14))");
			Assert.AreEqual(i.Console, "<Table ><Table.Columns><TableColumn Width=\"12\"/><TableColumn Width=\"13\"/>" +
			                "<TableColumn Width=\"14\"/></Table.Columns><TableRowGroup></TableRowGroup></Table>");
			                
			      
		}

		[Test]
		public void Test_04_Rows()
		{
			var i = GetI();
			i.Eval("(table (:columns 12 13) (row) (row))");
			Assert.AreEqual(i.Console, "<Table ><Table.Columns><TableColumn Width=\"12\"/><TableColumn Width=\"13\"/></Table.Columns>" +
			                "<TableRowGroup><TableRow ></TableRow><TableRow ></TableRow></TableRowGroup></Table>");
              
		}

		[Test]
		public void Test_05_Cells()
		{
			var i = GetI();
			i.Eval("(table (:columns 10) (row (cell)))");
			Assert.AreEqual("<Table ><Table.Columns><TableColumn Width=\"10\"/></Table.Columns>" +
			                "<TableRowGroup><TableRow ><TableCell ></TableCell></TableRow></TableRowGroup></Table>"
			               , i.Console);
		}

		[Test]
		public void Test_06_CellBorder()
		{
			var i = GetI();
			i.Eval("(cell (:border 1 0 1 0))");
			Assert.AreEqual(i.Console, "<TableCell BorderBrush=\"Black\" BorderThickness=\"1,0,1,0\" ></TableCell>");
		}
		
		[Test]
		public void Test_43_3dot()
		{
			var i = GetI();

			i.Eval(
				"(page (:page-padding 2 1 1 1)"+
				"	  (:page-height 29.7)"+
				"	  (:page-width 21)"+
				"	  (:font-size 12)"+
				"	  (:font-family \"Times New Roman\")"+
				"	(par \"тест\")"+
				"	(def (tmp)"+
				"		 (cell "+
				"		 	(par ...)"+
				"		 )"+
				"	)				"+
				"	(table (:columns 10 20)"+
				"	)"+
				")");
		}
	}
}
