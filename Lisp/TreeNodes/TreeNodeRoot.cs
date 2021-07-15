/*
 * Created by SharpDevelop.
 * User: Admin
 * Date: 21.10.2015
 * Time: 13:33
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;

namespace RLisp.TreeNodes
{
	/// <summary>
	/// Description of TreeNodeRoot.
	/// </summary>
	public class TreeNodeRoot: TreeNodeBase
	{
		public TreeNodeRoot(int aLine, int aCol):base(aLine, aCol)
		{
			Alias = "Root";
		}

		public override object Value{
			get { 
				return "";
			}			
		}

		public override string ToString()
		{
			return ToString(0);
		}

		public override string ToString(int aLevel)
		{
			StringBuilder sb = new StringBuilder();
			
			var ind = CreateIntend(aLevel);

			sb.AppendLine(ind + "Root");
			foreach(var c in Childs)
			{
				sb.AppendLine(c.ToString(aLevel + 1));
			}

			return sb.ToString().Trim();
		}
	}
}
