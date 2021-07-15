/*
 * Created by SharpDevelop.
 * User: Admin
 * Date: 21.10.2015
 * Time: 13:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace RLisp.TreeNodes
{
	/// <summary>
	/// Description of TreeNodeIdent.
	/// </summary>
	public class TreeNodeIdent: TreeNodeBase
	{
		public TreeNodeIdent(int aLine, int aCol, string aSource)
			: base(aLine, aCol, aSource)
		{
			Alias = "Ident";
			if( aSource.StartsWith("-") && aSource.Length > 1 ) //это не оператор-минус, это отрицательное число 
			{
				Source = aSource.Substring(1);
				Modifier = new MinusModifier();
			}
		}

		public override object Value{
			get{ 
				return Modifier.Modify(Env.GetVar(Line, Col, Source).Value);
			}
		}

		public override string ToString(int aLevel)
		{
			return CreateIntend(aLevel) + Source;
		}

		public override string ToString()
		{
			return ToString(0);
		}
		

	}
}
