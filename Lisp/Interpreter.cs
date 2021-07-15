using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace RLisp
{
	public class Interpreter
	{
		public Interpreter()
		{
			CultureInfo clone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			clone.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = clone;
			Env = new Environment();
			ClearConsole();
		}

		public string Console{
			get{
				return Environment.Console;
			}
		}

		public void ClearConsole()
		{
			Environment.ConsoleClear();
		}		

		public Environment Env{set;get;}
		public object Eval(string aSource)
		{
			var tree = Parser.GetTree(aSource, null);
			Debug.WriteLine(tree.ToString());
			object res = null;
			object resTmp;
			foreach(var e in tree.Childs)
			{
				e.Env = Env;
				resTmp = e.Value;
				if( resTmp != null )
					res = resTmp;
			}
			return res;
		}

		#region работа с JSON
		//private JObject _json = null;
		public void SetJSON(string aStr)
		{
			Env.Json = JObject.Parse(aStr);
		}

		#endregion
	}
}
