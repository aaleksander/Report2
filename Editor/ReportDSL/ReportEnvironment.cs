/*
 * Сделано в SharpDevelop.
 * Пользователь: aaleksander
 * Дата: 23.10.2015
 * Время: 21:07
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Linq;
using System.Reflection;
using RLisp;
using RLisp.TreeNodes;

namespace Editor.ReportDSL
{
	/// <summary>
	/// Всякие дополнительные штуки в окружение
	/// </summary>
	public class ReportEnvironment: RLisp.Environment
	{
		public ReportEnvironment(): base()
		{
			//переменные - "константы"
			SetVar("center", new TreeNodeString(0, 0, "Center"));
			SetVar("left", new TreeNodeString(0, 0, "Left"));
			SetVar("right", new TreeNodeString(0, 0, "Right"));
			SetVar("justify", new TreeNodeString(0, 0, "Justify"));

			//жирнота шрифта
			SetVar("bold", new TreeNodeString(0, 0, "Bold"));
			SetVar("normal", new TreeNodeString(0, 0, "Normal"));

			//добавляем в окружение все наши операторы
			Type ourType = typeof(XamlOperatorBase);
			var list = Assembly.GetAssembly(ourType)
				.GetTypes()
				.Where(type => type. IsSubclassOf(ourType))
				.OrderBy(x => x.Name);

			string tmp = "";
			foreach(var t in list)
			{
				tmp = t.Name.ToLower();
				tmp = tmp.Replace('_', '-');
				var obj = (IOperator) Activator.CreateInstance(t);
				(obj as XamlOperatorBase).Name = tmp;
				tmp = (obj as XamlOperatorBase).Prefix + tmp; //добавляем префикс
				_opers[tmp] = obj;
			}
		}
	}
}
