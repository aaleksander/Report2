using System;

namespace Lisp
{
	public enum TermType
	{
		LP, RP, 	//скобочки
		Identifier,	//идентификатор
		Number,		//число
		String		//TODO написать тесты
	}

	public class LispTerm
	{
		public LispTerm(TermType aType, string aSource)
		{
			Type = aType;
			Source = aSource;
		}

		public TermType Type{set;get;}
		
		/// <summary>
		/// строковой источник терма (каким он был в исходнике)
		/// </summary>
		public string Source{set;get;}

		public object Value{
			set;
			get;
		}
	}
}
