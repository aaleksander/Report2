using System;

namespace RLisp.TreeNodes
{
	/// <summary>
	/// Description of TreeNodeNumber.
	/// </summary>
	public class TreeNodeNumber: TreeNodeBase
	{
		public TreeNodeNumber(int aLine, int aCol, string aSource): base(aLine, aCol, aSource)
		{
			Alias = "Number";
			//Modifier = new StubModifier();
		}

		public override object Value {
			get { 
				if( Source.Contains(".") )
					return Modifier.Modify(double.Parse(Source));
				else
					return Modifier.Modify(int.Parse(Source));
			}
		}

		public override string ToString(int aLevel)
		{
			return CreateIntend(aLevel) + "val = " + Value;
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
