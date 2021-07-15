using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using RLisp.TreeNodes;

namespace RLisp
{
		/// <summary>
		/// команды, которые дает автомат парсеру
		/// </summary>
		public enum CommandType{
			None,
			/// <summary>
			/// конец программы
			/// </summary>
			End,
			/// <summary>
			/// нужно создать новый идентификатор
			/// </summary>
			Ident,
			/// <summary>
			/// новое число
			/// </summary>
			Number,
			/// <summary>
			/// новая строка
			/// </summary>
			String,
			/// <summary>
			/// Начало выражения
			/// </summary>
			BeginExpression,
			/// <summary>
			/// Конец выражения
			/// </summary>
			EndExpression,
			/// <summary>
			/// появилось многоточие
			/// </summary>
			Dots
		}
	
	public class Command
	{
		public CommandType Type{set;get;}
		public string Argument{set;get;}
		
		public override string ToString()
		{
			return string.Format("[{0} = {1}]", Type, Argument);
		}

	}
	
	
	/// <summary>
	/// Конечный автомат для анализа листинга LISP
	/// </summary>
	public class StateMachine
	{
		/// <summary>
		/// состояния машины
		/// </summary>
		public enum StateType
		{
			/// <summary>
			/// Ждем левую скобку
			/// </summary>
			WaitLP,

			/// <summary>
			/// Ждем идентикатор
			/// </summary>
			WaitIdentifier,

			/// <summary>
			/// читаем идентификатор
			/// </summary>
			ReadingIdent,

			/// <summary>
			/// читаем число
			/// </summary>
			ReadingNumber,

			/// <summary>
			/// читаем строку
			/// </summary>
			ReadingString,

			/// <summary>
			/// Ждем новый аргумент (скобочку, цифру, букву, что угодно)
			/// </summary>
			WaitArgument,

			/// <summary>
			/// однострочный комментарий
			/// </summary>
			OneLineComment,

			/// <summary>
			/// многоточие
			/// </summary>
			Dots,

			/// <summary>
			/// ожидаем пустое пространство
			/// </summary>
			WaitSpace
		}

		/// <summary>
		/// глубина скобочек, на которую погрузились в ходе разбора
		/// </summary>
		private int _level=0;

		public bool Isolete{private set; get;} //признак того, что включена изоляция следующего символа

		/// <summary>
		/// буфер, в котором копится материал для команды
		/// </summary>
		private StringBuilder _buffer = new StringBuilder();
		/// <summary>
		/// взять содержимое буфера и сбросить его
		/// </summary>
		/// <returns></returns>
		private string PushBuffer()
		{
			var tmp = _buffer.ToString();
			_buffer.Clear();
			return tmp;
		}

		public Stack<TreeNodeBase> Stack{private set; get;}

		public StateType State{set;get;}
		public int Line{set;get;}
		public int Col{set;get;}

		public StateMachine(string aSource)
		{
			Isolete = false;
			Line = 1;
			Col = 0;
			State = StateType.WaitLP;
			foreach(var a in aSource)
			{
				NextChar(a);
			}
		}

		public StateMachine()
		{
			Isolete = false;
			Line = 1;
			Col = 0;
			State = StateType.WaitLP;
		}

		public string Cache{
			set{
				_cache = value;
				_cacheIndex = 0;
			}
			get
			{
				return _cache;
			}
		}



		private string _cache = "";
		private int _cacheIndex = 0;
		public List<Command> NextChar()
		{
			if( _cacheIndex >= Cache.Length )
				return new List<Command>{new Command(){Type = CommandType.End}};
			char c = Cache[_cacheIndex];
			var res = NextChar(c);
			_cacheIndex++;
			return res;
		}
		
		
		/// <summary>
		/// пропустить определенное количесво символов из кэша
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public List<Command> NextChars(int a)
		{
			var res = new List<Command>();
			for(int i=0; i<a; i++)
			{
				res.AddRange(NextChar());
			}

			return res;
		}
		
		public List<Command> NextChars(string a)
		{
			var res = new List<Command>();
			foreach(var c in a)
			{
				res.AddRange(NextChar(c));
			}
			return res;
		}

		private static int _debugCount = 1;

		public List<Command> NextChar(char aChar)
		{
//			Debug.Write(_debugCount.ToString().PadLeft(5, ' ') + ": ");
//			Debug.Write("пришел символ: '" + aChar + "', ");
//			Debug.Write("State = " + State.ToString() + "; ");
//			Debug.WriteLine("Level = " + _level.ToString());
			_debugCount++;
			var res = new List<Command>();

			if( aChar == '\n' )
			{
				Line++;
				Col = 0;
			}
			else
			{
				Col++;
			}

			if( State == StateType.OneLineComment && aChar != '\r' && aChar != '\n' )
			{//до конца строки ни на что внимания не обращаем
				return res;
			}

			switch(aChar)
			{
				case '\n': case ' ': case '\t': case '\r':
					res = Space(aChar);
					break;

				case '(': 
					res = LP();
					break;

				case ')': 
					res = RP();
					break;

				case '0': case '1': case '2': 
				case '3': case '4': case '5':
				case '6': case '7': case '8':
				case '9': 
					res = Number(aChar);
					break;

				case '.': 
					res = Dot();
					break;

				case '\'':
				case '"': 
					res = this.String();
					break;

				case '-': 
					res = Minus();
					break;

				case '\\': //изоляция следующего символа
					if( Isolete == false )
					{
						Isolete = true;
					}
					break;
				case ';': //однострочный комментарий
					res = OneLineComm();
					break;
				default:
					res = Other(aChar);
					break;
			}

			return res;
		}

		private List<Command> OneLineComm()
		{
			var res = new List<Command>();
			switch(State)
			{
				case StateType.ReadingString:
					_buffer.Append(";");
					break;
				default:
					State = StateType.OneLineComment;
					break;
			}
			return res;
		}

		private List<Command> Minus()
		{
			switch(State)
			{
				case StateType.WaitSpace:
					throw new Exception("Ожидается пустой символ-разделитель");

				case StateType.WaitIdentifier:
				case StateType.WaitArgument:
				case StateType.ReadingString:
				case StateType.ReadingIdent:
					_buffer.Append("-");
					break;
			}
			return new List<Command>();
		}

		private List<Command> String()
		{
			switch(State)
			{					
				case StateType.WaitSpace:
					throw new Exception("Ожидается пустой символ-разделитель");

				case StateType.ReadingString: //заканчиваем читать строку
					if( Isolete == true ) //включена изоляция. Надо просто забрать кавычку в буфер
					{
						_buffer.Append("\"");
						Isolete = false;
						break;
					}
					else
					{
						State = StateType.WaitArgument;
						return new List<Command>{
							new Command(){
								Type = CommandType.String,
								Argument = PushBuffer()
							}};
					}

				case StateType.WaitArgument:
					State = StateType.ReadingString;
					break;
			}

			return new List<Command>();
		}

		private List<Command> Dot()
		{
			switch(State)
			{
				case StateType.WaitArgument: //многоточие
					_buffer.Append('.');					
					State = StateType.Dots;
					break;

				case StateType.Dots:
					_buffer.Append('.');					
					if( _buffer.Length > 4 )
						throw new Exception("Слишком много точек");
					if( _buffer.Length == 3 )
					{
						State = StateType.WaitSpace;
						return new List<Command>{new Command(){Type = CommandType.Dots}};
					}
					break;
					
				case StateType.ReadingString:
				case StateType.ReadingIdent: 
					_buffer.Append('.');
					break;
				case StateType.ReadingNumber:
					//проверим, к месту ли эта точка (вдруг вторая по счету)
					_buffer.Append('.');
					try
					{
						var tmp = double.Parse(_buffer.ToString() + "0");
					}catch(FormatException)
					{
						throw new NumberIsWrongException(Line, Col);
					}catch(OverflowException)
					{
						throw new NumberToBigException(Line, Col);
					}
					break;
			}
			return new List<Command>();//{new Command(){Type=CommandType.None}};
		}

		private List<Command> LP()
		{
			switch(State)
			{
				case StateType.WaitSpace:
					throw new Exception("Ожидается пустой символ-разделитель");
				case StateType.ReadingString:
					_buffer.Append("(");
					break;
				default:
					_level++; //погружаемся
					State = StateType.WaitIdentifier;
					return new List<Command>{new Command(){Type=CommandType.BeginExpression}};
			}
			return new List<Command>();
		}

		private List<Command> RP()
		{
			var res = new List<Command>();
			switch(State)
			{
				case StateType.WaitSpace:
					_buffer.Clear();
					break;

				case StateType.ReadingString:
					_buffer.Append(")");
					return new List<Command>();

				case StateType.WaitLP:
					_level--; //всплываем
					if( _level != 0 )
					{
						throw new MachRPException(Line, Col);
					}
					break;

				case StateType.ReadingIdent:
					_level--; //всплываем
					res.Add(new Command(){
					        	Type = CommandType.Ident,
					        	Argument = PushBuffer()
					        });
					break;

				case StateType.ReadingNumber:
					_level--; //всплываем
					res.Add(new Command(){
					        	Type = CommandType.Number,
					        	Argument = PushBuffer()
					        });
					break;				
			}

			State = (_level == 0)? StateType.WaitLP: StateType.WaitArgument;
			res.Add(new Command(){Type=CommandType.EndExpression});
			return res;//new List<Command>{new Command(){Type=CommandType.EndExpression}};
		}

		private List<Command> Other(char aChar)
		{
			switch(State)
			{
				case StateType.WaitLP: //ждем '(', а пришла хрень
					throw new NoLPException(Line, Col);
				case StateType.ReadingString:

					break;
				default:
					State = StateType.ReadingIdent;
					break;
			}

			_buffer.Append(aChar);

			return new List<Command>();//{new Command(){Type=CommandType.None}};
		}

		private List<Command> Number(char aChar)
		{
			switch(State)
			{
				case StateType.WaitSpace:
					throw new Exception("Ожидается пустой символ-разделитель");

				case StateType.ReadingString: //читаем строку
				case StateType.ReadingIdent: //идентификатор с цифрами
					_buffer.Append(aChar);
					return new List<Command>();
				case StateType.WaitIdentifier: //мы ждем идентификатор
					throw new NoIdentException(Line, Col);
			}
			State = StateType.ReadingNumber;
			_buffer.Append(aChar);
			return new List<Command>();//{new Command(){Type=CommandType.None}};
		}

		public List<Command> Space(char aCh)
		{
			switch(State)
			{
				case StateType.WaitSpace:
					_buffer.Clear();
					State = StateType.WaitArgument;
					break;

				case StateType.OneLineComment:
					if( aCh == '\r' || aCh == '\n' )
						State = StateType.WaitArgument;
					break;

				case StateType.WaitIdentifier:
					if( _buffer.ToString() == "-") //минус как оператор
					{
						State = StateType.WaitArgument;
						return new List<Command>{
							new Command(){
								Type = CommandType.Ident,
								Argument = PushBuffer()}};
					}
					break;

				case StateType.ReadingIdent: 
					State = StateType.WaitArgument;
					return new List<Command>{
						new Command(){
							Type = CommandType.Ident,
							Argument = PushBuffer()}};

				case StateType.ReadingNumber: 
					State = StateType.WaitArgument;
					return new List<Command>{
						new Command(){
							Type = CommandType.Number,
							Argument = PushBuffer()}};

				case StateType.ReadingString:
					_buffer.Append(" ");
					break;
			}

			return new List<Command>();//{new Command(){Type = CommandType.None}};
		}
	}
}
