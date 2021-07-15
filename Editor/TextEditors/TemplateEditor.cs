using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace Editor.TextEditors
{
	/// <summary>
	/// Description of TemplateEditor.
	/// </summary>
	public class TemplateEditor: MVVMTextEditor
	{
		public TemplateEditor():base()
		{
			TextArea.TextEntering += TextEntering;
			TextArea.TextEntered += TextEntered;
		}

		#region автозавершение
		#region вспомогательный класс
		
		public class MyCompletionData: ICompletionData
		{
			public string Text{set;get;}
			public MyCompletionData(string text, string aDescr)
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
		#endregion


		/// <summary>
		/// Пройти к началу от aOffset и найти строку до символа aStopChar
		/// </summary>
		/// <param name="aOffset"></param>
		/// <param name="aStopChar"></param>
		/// <returns></returns>
		private string GetText(int aOffset, params char[] aStopChars)
		{
			string res = "";
			for(int i=aOffset; i>=0; i--)
			{
				foreach(char c in aStopChars) //проверяем, не пора ли остановиться
				{
					if( Text[i] == c )
					{
						return res;
					}
				}
				res = Text[i] + res;
			}
			return res;
		}

		CompletionWindow _completionWindow = null;
		void TextEntered(object sender, TextCompositionEventArgs e)
		{
//			if( e.Text == "." )
//			{
//				var txt = GetText(TextArea.Caret.Offset - 2, ':', '{');	//узнаем полный пункт
//				JToken tmp1;
//				try{
//					tmp1 = _json.SelectToken(_json.MyPathToSelectTokenPath(txt));
//				}catch(Exception)
//				{
//					tmp1 = null;
//				}
//				if( tmp1 == null )
//					return;
//				var ch1 = tmp1.Children();
//
//				_completionWindow = new CompletionWindow(TextArea);
//				IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
//				Type t = typeof(JProperty);
//				var prop = t.GetProperty("Name");
//				foreach(var r in ch1)
//				{
//					try
//					{
//						data.Add(new MyCompletionData((string)prop.GetValue(r, null), r.ToString()));
//					}catch(Exception)
//					{
//						data.Add(new MyCompletionData("Нет свойтв", ""));
//					}
//				}
//				_completionWindow.Show();
//				_completionWindow.Closed += delegate { _completionWindow = null; };
//			}
//
//			if( e.Text == "<" ) //тэги
//			{
//				_completionWindow = new CompletionWindow(TextArea);
//				IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
//				data.Add(new MyCompletionData("Table", "Таблица"));
//				data.Add(new MyCompletionData("Paragraph", "Параграф"));
//				_completionWindow.Show();
//				_completionWindow.Closed += delegate { _completionWindow = null; };
//			}
//
//			if( e.Text == ":" || e.Text == "{" )//форматеры и округления
//			{
//				_completionWindow = new CompletionWindow(TextArea);
//				IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
//				data.Add(new MyCompletionData("-", "вставить цифры через тире (для детекторов)"));
//				data.Add(new MyCompletionData("DT", "вставить timestamp как дату (без времени)"));
//				data.Add(new MyCompletionData("T", "вставить timestamp как время (без даты)"));
//				data.Add(new MyCompletionData("AsText", "вставить число прописью"));
//				data.Add(new MyCompletionData("AsTextTons", "вставить число прописью и добавить сдать 'тонн/тонны'"));
//				data.Add(new MyCompletionData("DTR", "вставить timestamp как дату с месяцем в родительном падеже"));
//				data.Add(new MyCompletionData("DTM", "вставить timestamp как дату вплоть до минут"));
//				data.Add(new MyCompletionData("AsIsDouble", "вставить double как есть, заменить точку на запятую"));
//				
//				
//				
//				
//				var tmp = _json.Children();
//				Type t = typeof(JProperty);
//				var prop = t.GetProperty("Name");
//				foreach(var r in tmp)
//				{
//					data.Add(new MyCompletionData((string)prop.GetValue(r, null), r.ToString()));
//				}
//				_completionWindow.Show();
//				_completionWindow.Closed += delegate { _completionWindow = null; };
//			}
		}

		void TextEntering(object sender, TextCompositionEventArgs e)
		{
			if( e.Text.Length > 0 && _completionWindow != null )
			{
				if( !char.IsLetterOrDigit(e.Text[0]) )
				{
					_completionWindow.CompletionList.RequestInsertion(e);
				}
			}
		}
		#endregion

		#region folding
		public XmlFoldingStrategy FoldingStrategy{
			get{
				if( _foldingStrategy == null )
				{
					_foldingStrategy = new XmlFoldingStrategy();
				}
				return _foldingStrategy;
			}
		}
		private XmlFoldingStrategy _foldingStrategy = null;

		public FoldingManager FoldingManager{
			get{
				if( _foldingManager == null )
				{
					_foldingManager = FoldingManager.Install(this.TextArea);
				}
				return _foldingManager;
			}
		}
		private FoldingManager _foldingManager = null;
		#endregion
		
		protected override void OnTextChanged(EventArgs e)
		{
			FoldingStrategy.UpdateFoldings(FoldingManager, Document);
			RaisePropertyChanged("Length");
			base.OnTextChanged(e);
		}
		
		public void ReloadHighLighting()
		{
			var tmp = new ColorizeAvalonEdit();
			
			this.TextArea.TextView.LineTransformers.Add(tmp);
			var bb = new BracketHighlightRenderer(this);
			bb.UpdateColors(Colors.Yellow, Colors.YellowGreen);
			Stream xshd_stream = null;
			XmlTextReader xshd_reader = null;
			try
			{
				xshd_stream = File.OpenRead("Highlighting.xml");
				xshd_reader = new XmlTextReader(xshd_stream);
				
				// Apply the new syntax highlighting definition.
				this.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(
				    xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance
				);
			}catch(Exception e)
			{
				MessageBox.Show(e.Message);
			}
			finally
			{
				if( xshd_reader != null )
					xshd_reader.Close();
				if( xshd_stream == null )
					xshd_stream.Close();
			}
		}
	}
}
