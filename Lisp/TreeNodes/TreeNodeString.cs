/*
 * Created by SharpDevelop.
 * User: Admin
 * Date: 21.10.2015
 * Time: 15:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RLisp.TreeNodes
{
	/// <summary>
	/// Description of TreeNodeString.
	/// </summary>
	public class TreeNodeString: TreeNodeBase
	{
		public TreeNodeString(int aLine, int aCol, string aSource): base(aLine, aCol, aSource)
		{
			Alias = "String";
		}

		public override object Value {
			get { 
				return Source; 
			}
		}

		public override string ToString(int aLevel)
		{
			return CreateIntend(aLevel) + "val = '" + Source + "'";
		}

		public override string ToString()
		{
			return ToString(0);
		}

		public override void SetSource(int aLine, int aCol, string aNewVal)
		{
			Source = aNewVal;
		}
	}
}
