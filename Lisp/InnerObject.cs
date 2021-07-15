using System;
using RLisp.TreeNodes;

namespace RLisp
{
	/// <summary>
	/// Внутренний объект для хранения разных значений
	/// </summary>
	public class InnerObject: TreeNodeBase
	{
		public InnerObject(object aObj): base(0, 0)
		{
			_obj = aObj;
		}
		
		private object _obj = null;
		public override object Value{
			get{
				return _obj;
			}
		}
	}
}
