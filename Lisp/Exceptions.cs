/*
 * Сделано в SharpDevelop.
 * Пользователь: aaleksander
 * Дата: 20.10.2015
 * Время: 19:30
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;

namespace RLisp
{
	/// <summary>
	/// Базовый класс для всех исключения
	/// </summary>
	public class BaseException: Exception
	{
		public int Line{private set;get;}
		public int Col{private set;get;}
		//public string Message{private set;get;}
		public BaseException(int aLine, int aCol, string aMess):base(aMess + ": line=" + aLine.ToString() + 
		                                                                     "; col=" + aCol.ToString())
		{
			Line = aLine;
			Col = aCol;
		}
	}

	public class NoLPException: BaseException
	{
		public NoLPException(int aLine, int aCol)
			:base(aLine, aCol, "Ожидается '('")
		{
		}
	}

	public class NoIdentException: BaseException
	{
		public NoIdentException(int aLine, int aCol)
			:base(aLine, aCol, "Ожидается идентификатор")
		{
		}
	}
	
	public class NumberIsWrongException: BaseException
	{
		public NumberIsWrongException(int aLine, int aCol)
			:base(aLine, aCol, "Неправильный формат числа")
		{
		}
	}
	
	public class NumberToBigException: BaseException
	{
		public NumberToBigException(int aLine, int aCol)
			:base(aLine, aCol, "Число слишком большое или маленькое")
		{
		}
	}
	
	public class MachRPException: BaseException
	{
		public MachRPException(int aLine, int aCol)
			:base(aLine, aCol, "Лишняя ')'")
		{
		}
	}
	
	public class OopsException: BaseException
	{
		public OopsException(int aLine, int aCol)
			:base(aLine, aCol, "Неожиданное завершение программы")
		{
		}
	}
	
	public class EpmtyExpressionException: BaseException
	{
		public EpmtyExpressionException(int aLine, int aCol)
			:base(aLine, aCol, "Выражение пустое")
		{
		}
	}
	
}
