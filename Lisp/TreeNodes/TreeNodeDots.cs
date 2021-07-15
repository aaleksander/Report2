using System;

namespace RLisp.TreeNodes
{
	public class TreeNodeDots: TreeNodeBase
	{
		public TreeNodeDots(int aLine, int aCol): base(aLine, aCol)
		{
			Alias = "ParPlace";
		}

		public override string ToString()
		{
			return "...";
		}

		public override string ToString(int aLevel)
		{
			return CreateIntend(aLevel) + "dots";
		}
	}
}
