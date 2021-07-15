using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;

namespace RLisp.TextEditors
{
	public class ColorizeAvalonEdit : DocumentColorizingTransformer
	{
	    protected override void ColorizeLine(DocumentLine line)
	    {
	        int lineStartOffset = line.Offset;
	        string text = CurrentContext.Document.GetText(line);
	        int start = 0;
	        int index;
	        while ((index = text.IndexOf("AvalonEdit", start)) >= 0) {
	            base.ChangeLinePart(
	                lineStartOffset + index, // startOffset
	                lineStartOffset + index + 10, // endOffset
	                (VisualLineElement element) => {
	                    // This lambda gets called once for every VisualLineElement
	                    // between the specified offsets.
	                    Typeface tf = element.TextRunProperties.Typeface;
	                    // Replace the typeface with a modified version of
	                    // the same typeface
	                    element.TextRunProperties.SetTypeface(new Typeface(
	                        tf.FontFamily,
	                        FontStyles.Italic,
	                        FontWeights.Bold,
	                        tf.Stretch
	                    ));
	                });
	            start = index + 1; // search for next occurrence
			}   
	    }   
	}


	/// <summary>
	/// Description of TemplateEditor.
	/// </summary>
	public class TemplateEditor: MVVMTextEditor
	{
		public TemplateEditor():base()
		{
			TextArea.TextEntering += TextEntering;
			TextArea.TextEntered += TextEntered;
			
			TextArea.Caret.PositionChanged += OnCaretChanged;
		}

		#region автозавершение

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
			//var res = Completer.GetContext(Text + e.Text);
			RLispCompleter c = null;
			if( e.Text == "(" || e.Text == " " )
			{
				c = new RLispCompleter(Text.Substring(0, TextArea.Caret.Offset) + e.Text);
				if( c.Data.Count == 0 )
				{
					return;
				}
			}


			if( c == null )
			{
				return;
			}

			_completionWindow = new CompletionWindow(TextArea);
			IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
			foreach(var d in c.Data)
			{
				data.Add(d);
			}
			_completionWindow.Show();
			_completionWindow.Closed += delegate { _completionWindow = null; };
			
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
		
		private void OnCaretChanged(object sender, object e)
		{
			var s = sender as Caret;
			string ch;
			try{
				ch = Text.Substring(s.Offset, 1);
				TextArea.TextView.InvalidateMeasure();
			}catch(Exception)
			{
				
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			FoldingStrategy.UpdateFoldings(FoldingManager, Document);
			RaisePropertyChanged("Length");
			base.OnTextChanged(e);
		}

		/// <summary>
		/// загрузка цветной схемы
		/// </summary>
		public void ReloadHighLighting()
		{
			var tmp = new ColorizeAvalonEdit();

			//это для подсветки парных скобочек
			this.TextArea.TextView.LineTransformers.Add(tmp);
			var bb = new BracketHighlightRenderer(this);
			bb.UpdateColors(Colors.Yellow, Colors.YellowGreen);

			Stream xshd_stream = null;
			XmlTextReader xshd_reader = null;
			//TODO надо сделать через код, чтобы xml-файл с собой не таскать
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
