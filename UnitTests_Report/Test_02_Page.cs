using System;
using NUnit.Framework;

namespace UnitTests_Report
{
	[TestFixture]
	public class Test_02_Page: Test_00
	{
		[Test]
		public void Test_01()
		{
			var i = GetI();

			i.Eval("(page )");
			Assert.AreEqual(i.Console, "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
    			"xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +  
    			"ColumnWidth=\"99999\" ></FlowDocument>");
		}

		[Test]
		public void Test_02_PagePadding()
		{
			var i = GetI();

			i.Eval("(page (:page-padding 2 0.5 0.5 1))");
			Assert.AreEqual(i.Console, "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
    			"xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +  
    			"ColumnWidth=\"99999\" PagePadding=\"2cm,0.5cm,0.5cm,1cm\" ></FlowDocument>");
		}
	}
}
