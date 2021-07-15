using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Model
{
	public class ModelBase
	{
		public ModelBase()
		{
		}
		
		/// <summary>
		/// Возвращает готовый запрос вида select * from ....
		/// </summary>
		/// <returns></returns>
/*		public static Sql Select
		{
			get{
				return Sql.Builder
					.Select("*")
					.From(TableName);
			}
		}*/

/*		public static Sql SelectByRN(int aRN)
		{
			return Sql.Builder
				.Select("*")
				.From(TableName)
				.Where("RN = " + aRN.ToString());
		}*/
		
/*		public static Sql SelectByOther(string aField, int aRN)
		{
			return Sql.Builder
				.Select("*")
				.From(SQLFactory.GetTableName<)
				.Where(aField + " = " + aRN.ToString());
		}		*/

		/// <summary>
		/// имя таблицы БД
		/// </summary>
/*		public static string TableName
		{
			get{
				//ищем аттрибуд TableName	
				//берем весь стэк вызовов				
				StackTrace st = new StackTrace(false);
				string tableName = "";
				for(int i = st.FrameCount - 1; i >= 0; i--) //перебираем от дочерних классов к базовым
				{
					//определяем типы во всех стэках					
					StackFrame frame = st.GetFrame(i);
					MethodBase mi = frame.GetMethod();
					Type declaringType = mi.DeclaringType;
	
					System.Attribute[] attrs = System.Attribute.GetCustomAttributes( declaringType );
					tableName = "";
			        foreach (System.Attribute attr in attrs )
			        {
			            if (attr is TableNameAttribute)
			            {//мы нашли нужный аттрибут
			                tableName = ((TableNameAttribute) attr).Value;
							break;
			            }
			        }
			        if( tableName != "" )
			        	break;
				}
				
		        if( tableName == "" )		        	
		        	throw new NoTableNameAttributeException();
		        
		        return tableName;
			}		
		}*/
		

	}
}
