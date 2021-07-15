using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Editor.ReportDSL;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using RLisp.ReportDSL;
using RLisp.TreeNodes;

namespace RLisp.TextEditors
{
	
	public class CompletionData: ICompletionData
	{
		public string Text{set;get;}
		public CompletionData(string text, string aDescr)
		{
			Text = text;
			Description = aDescr;
		}
		
		public System.Windows.Media.ImageSource Image{
			get{
				return null;
			}
		}
		
		public object Content{
			get{
				return Text;
			}				
		}

		public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ISegment completioinSegment, EventArgs insertEA)
		{
			textArea.Document.Replace(completioinSegment, Text);
		}

		public object Description{
			set;get;
		}

		public double Priority{
			get{
				return 1;
			}
		}
	}
	
	
	/// <summary>
	/// тип контекста
	/// </summary>
	public enum ContextType{
		/// <summary>
		/// ничего не происходит
		/// </summary>
		none, 
		/// <summary>
		/// ввели левую скобочку
		/// </summary>
		LP,
	
		/// <summary>
		/// ввели пробел
		/// </summary>
		space
	};

	/// <summary>
	/// класс описывает состояние в произвольном месте кода
	/// </summary>
	public class CodeContext
	{
		public CodeContext(ContextType aType)
		{
			Type = aType;
		}

		public ContextType Type{private set;get;}

		public Stack<TreeNodeBase> Stack{
			get{
				return _stack;
			}
		}
		Stack<TreeNodeBase> _stack = new Stack<TreeNodeBase>();
	}

	/// <summary>
	/// Класс для автозавершения кода
	/// </summary>
	public class CompleterBase
	{
		public CompleterBase()
		{}
		
		CodeContext _cont = null;
		public CompleterBase(string aSource)
		{
			_cont = CompleterBase.GetContext(aSource);
		}

		/// <summary>
		/// получить контекст из кода
		/// </summary>
		/// <param name="aSource">код от начала до текущие позиии</param>
		/// <returns></returns>
		public static CodeContext GetContext(string aSource)
		{
			CodeContext res = null;
			switch(aSource.Last())
			{
				case '(': res = new CodeContext(ContextType.LP); break;
				case ' ': res = new CodeContext(ContextType.space); break;
				default: res = new CodeContext(ContextType.none); break;
			}

			//заполняем стэк
			try
			{
				var t = Parser.GetTree(aSource, res.Stack);
			}catch(Exception){}

			return res;
		}

		protected List<Tuple<string, CompletionData>> _data = new List<Tuple<string, CompletionData>>();

		public List<CompletionData> Data{
			get{
				var res = new List<CompletionData>();
				foreach(var a in _data)
				{
					if( _cont.Stack.Count == 0 ) //если стэк пустой, то это корень
					{
						if( a.Item1 == "" )
						{
							res.Add(a.Item2);
						}
					}
					else
					{
						switch(_cont.Type)
						{
							case ContextType.LP:
								if( a.Item1 == _cont.Stack.Peek().Source )
								{
									res.Add(a.Item2);
								}
								break;
							case ContextType.space:
								if( a.Item1 == _cont.Stack.Peek().Source && _cont.Stack.Peek().Source.StartsWith(":") )
								{
									res.Add(a.Item2);
								}
								break;
						}
					}
				}
				//отсортировать по имени
				res.Sort(new Comparer());
				return res; 
			}
		}

		private class Comparer:IComparer<CompletionData>
		{
			public int Compare(CompletionData a, CompletionData b)
			{
				return a.Text.CompareTo(b.Text);
			}
		}

		/// <summary>
		/// добавляет шаблон для автозавершения
		/// </summary>
		/// <param name="aOperator">для какого оператора</param>
		/// <param name="aParam">название команды которая будет добавляться</param>
		/// <param name="aDescription">примечание для команды</param>
		public void AddData(string aOperator, string aParam, string aDescription)
		{
			_data.Add(new Tuple<string, CompletionData>(aOperator, new CompletionData(aParam, aDescription)));
		}

		/// <summary>
		/// добавляет сразу несколько завершений для оператора
		/// </summary>
		/// <param name="aOperator"></param>
		/// <param name="aData"></param>
		public void AddData(string aOperator, params CompletionData[] aData)
		{
			foreach(var d in aData)
			{
				_data.Add(new Tuple<string, CompletionData>(aOperator, d));
			}
		}
	}

	public class RLispCompleter:CompleterBase
	{
		public RLispCompleter(string a):base(a)
		{
			//корень
			AddData("", "page", "страница отчета");
			AddData("", "def", "Определение функции или переменной");
			
			Type ourType = typeof(XamlOperatorBase);
			var list = Assembly.GetAssembly(ourType)
				.GetTypes()
				.Where(type => type. IsSubclassOf(ourType))
				.OrderBy(x => x.Name);

			foreach(var t in list)
			{
				var obj = (XamlOperatorBase) Activator.CreateInstance(t);

				var props = (CanParamAttribute)Attribute.GetCustomAttribute(t, typeof(CanParamAttribute));
				if( props != null )
				{
					foreach(var c in props.List)
					{
						var tmp = (IOperator) Activator.CreateInstance(c);

						//узнаем описание класса
						var descr = (DescriptionAttribute)Attribute.GetCustomAttribute(c, typeof(DescriptionAttribute));
						var txt = descr == null? "нет описания": descr.Text;

						AddData(obj.Alias, tmp.Alias, txt);
					}
				}

				var props2 = (CanChildAttribute)Attribute.GetCustomAttribute(t, typeof(CanChildAttribute));
				if( props2 != null )
				{
					foreach(var c in props2.List)
					{
						var tmp = (IOperator) Activator.CreateInstance(c);

						//узнаем описание класса
						var descr2 = (DescriptionAttribute)Attribute.GetCustomAttribute(c, typeof(DescriptionAttribute));
						var txt2 = descr2 == null? "нет описания": descr2.Text;

						AddData(obj.Alias, tmp.Alias, txt2);
					}
				}
			}
		}
	}
}
