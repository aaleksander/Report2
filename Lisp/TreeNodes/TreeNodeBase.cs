using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RLisp.TreeNodes;

namespace RLisp.TreeNodes
{
	/// <summary>
	/// Узел дерева распарсенного исходника
	/// </summary>
	public class TreeNodeBase
	{
		public TreeNodeModifier Modifier{protected set; get;}

		public virtual object Value{
			get{
				throw new Exception("Нельзя взять Value у базового класса");
			}
			set{
				_value = value;
			}
		}
		private object _value = null;

		public int Line{set;get;}
		public int Col{set;get;}

		protected TreeNodeBase(int aLine, int aCol)
		{
			Line = aLine;
			Col = aCol;
			Childs = new List<TreeNodeBase>();
			Modifier = ModifierFactory.GetStub();
		}

		/// <summary>
		/// ищет у себя узел
		/// </summary>
		/// <param name="aPredicate"></param>
		/// <returns></returns>
		public TreeNodeBase FindNode(Func<TreeNodeBase, bool> aPredicate)
		{
			if( aPredicate(this) ) //этож я!
			{
				return this;
			}

			TreeNodeBase res = null;
			if( Childs != null )
			{				
				foreach(var c in Childs)
				{
					res = c.FindNode(aPredicate);
					if( res != null )
						break;
				}
			}

			return res;
		}

		protected TreeNodeBase(int aLine, int aCol, string aS)//, NodeType aT) //это обычный аргрумент
		{
			Line = aLine;
			Col = aCol;
			Source = aS;
			Modifier = ModifierFactory.GetStub();//new StubModifier();
		}

		protected TreeNodeBase(int aLine, int aCol, ref StateMachine st, Stack<TreeNodeBase> aStack)
		{//Если мы здесь, значит StateMachine уже напоролась на '('
			Line = aLine;
			Col = aCol;
			Childs = new List<TreeNodeBase>();
			List<Command> coms = new List<Command>();
			string str;
			var com = new Command(){
				Type = CommandType.None
			};

			while( com.Type != CommandType.EndExpression )
			{
				foreach(var c in coms)
				{
					com = c;
					switch(com.Type)
					{
						case CommandType.End:							
							throw new OopsException(st.Line, st.Col);
						case CommandType.None:
							break;
						case CommandType.Ident:
							str = com.Argument;
							var ident = new TreeNodeIdent(st.Line, st.Col - str.Length, str);
							if( Childs.Count == 0) //это первый параметр, значит - это оператор
							{
								if( aStack != null )
									aStack.Push(ident);
							}
							Childs.Add(ident);
							break;
						case CommandType.Number:
							str = com.Argument;
							Childs.Add(new TreeNodeNumber(st.Line, st.Col, str));//, NodeType.Number));
							break;
						case CommandType.BeginExpression:
							Childs.Add(new TreeNodeExpression(st.Line, st.Col, ref st, aStack));
							break;
						case CommandType.EndExpression:
							if( aStack != null ) 
								aStack.Pop(); //выражение законченно, нужно выйти из стэка
							return; ///создание этого узла законченно
						case CommandType.String:
							str = com.Argument;
							Childs.Add(new TreeNodeString(st.Line, st.Col - str.Length, str));
							break;
						case CommandType.Dots:
							Childs.Add(new TreeNodeDots(st.Line, st.Col - 3));
							break;
					}
				}
				coms = st.NextChar();
			}
			//выражение законченно, нужно выйти из стэка
			if( aStack != null )
				aStack.Pop();
			Modifier = ModifierFactory.GetStub();//new StubModifier();
		}

		public string Alias{protected set; get;}

		public Environment Env = new Environment(); //у каждого выражения - свое окружение

		public List<TreeNodeBase> Childs{set;get;}

		public string Source{protected set;get;}

		public virtual void SetSource(int aLine, int aCol, string aNewVal)
		{
			throw new BaseException(aLine, aCol, "нельзя присвоить новое значение");
		}
		
		public int Count{
			get{
				
				return (Childs == null)? 0: Childs.Count;
			}
		}

		public virtual string ToString(int aLevel)
		{
			return CreateIntend(aLevel) + this.GetType().ToString() + "(base)";
		}

		public override string ToString()
		{
			return ToString(0);
		}

		protected string CreateIntend(int aSize)
		{
			StringBuilder indSB = new StringBuilder();
			for(int i=0; i<aSize; i++)
			{
				indSB.Append("  ");
			}
			return indSB.ToString();
		}
	}
}
