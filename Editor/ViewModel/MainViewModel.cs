using System;
using System.IO;
using System.Text;
using Editor.ReportDSL;
using Model;
using RLisp;

namespace ViewModel
{
	public class MainViewModel: ViewModelBase<ModelBase>
	{
		public MainViewModel()
		{
			RLisp = new Interpreter();
			RLisp.Env = new ReportEnvironment();

			if( File.Exists("report.txt" ) == false )
			{
				File.WriteAllText("report.txt", "(page (:page-padding 2 1 1 1)\r\n\t  (:page-height 29.7)\r\n\t  " +
				                  				"(:page-width 21)\r\n\t  " +
				                  				"(:font-size 12)\r\n\t  " +
												"(:font-family \"Times New Roman\")\r\n \r\n" +
												"\t(par \"тест\")\r\n\r\n)");
			}

			if( File.Exists("data.json" ) == false )
			{
				File.WriteAllText("data.json", "{}");
			}

			_source = File.ReadAllText("report.txt", Encoding.UTF8);;

			_json = File.ReadAllText("data.json", Encoding.UTF8);
		}

		private Interpreter RLisp{ set; get;}

		public string Source{
			set{
				_source = value;
				onPropertyChanged("Source");
				RLisp.ClearConsole();
				RLisp.Env = new ReportEnvironment();
				try
				{
					RLisp.SetJSON(_json);
					RLisp.Eval(value);
					Errors = "";
				}catch(Exception e)
				{
					Errors = e.Message + "\n" + e.StackTrace;
					throw e;
				}
				this.Console = RLisp.Console;
			}
			get{
				return _source;
			}
		}
		private string _source = "";

		public string Json{
			set{				
				_json = value;
				onPropertyChanged("Json");
			}
			get{
				return _json;
			}
		}
		private string _json = "";

		public string Console{
			set{
				_console = value;
				onPropertyChanged("Console");
			}
			get{
				return _console;
			}
		}
		private string _console = "";

		public string Errors{
			set{
				_errors = value;
				onPropertyChanged("Errors");
			}
			get{
				return _errors;
			}
		}
		private string _errors = "";
	}
}
