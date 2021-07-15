using System;

namespace RLisp.TreeNodes
{
	/// <summary>
	/// модификатор для узла дерева
	/// </summary>
	public class TreeNodeModifier
	{
		protected TreeNodeModifier(){}

		public static TreeNodeModifier Create(TreeNodeBase aObj, string aSource = "")
		{
			if( aObj is TreeNodeNumber ) //числам не нужны модификаторы
				return ModifierFactory.GetStub();//new StubModifier();

			return ModifierFactory.GetStub();//new  StubModifier();
		}

		public virtual object Modify(object aVal)
		{
			throw new Exception();
		}
	}

	
	public static class ModifierFactory
	{
		public static TreeNodeModifier GetStub()
		{
			return _stub;
		}
		private static TreeNodeModifier _stub = new StubModifier();
		
		public static TreeNodeModifier GetMinus()
		{
			return _minus;
		}
		private static TreeNodeModifier _minus = new MinusModifier();
		
	}
	
	/// <summary>
	/// модификатор по умолчанию. Ничего не делает
	/// </summary>
	public class StubModifier: TreeNodeModifier
	{
		public StubModifier(){
		
		}
		public override object Modify(object aVal)
		{
			return aVal;
		}
	}

	/// <summary>
	/// модификатор делает значение отрицательным (если может)
	/// </summary>
	public class MinusModifier: TreeNodeModifier
	{
		public MinusModifier()
		{
			
		}
		
		public override object Modify(object aVal)
		{
			if( aVal is int )
			{
				return -(int)aVal;
			}
			throw new Exception("я не знаю как это заминусовать:"
			                    + aVal.GetType().ToString());
		}
		
	}
}
