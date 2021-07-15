/*
 * Created by SharpDevelop.
 * User: Admin
 * Date: 24.10.2015
 * Time: 13:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Editor.ReportDSL;
using NUnit.Framework;
using RLisp;

namespace UnitTests_Report
{
//	[TestFixture]
	public class Test_00
	{
		protected Interpreter GetI()
		{
			var i = new Interpreter();
			i.Env = new ReportEnvironment();
			return i;
		}

	}
}
