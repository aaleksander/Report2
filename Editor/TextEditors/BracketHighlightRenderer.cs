/*
 * Сделано в SharpDevelop.
 * Пользователь: User
 * Дата: 05.11.2015
 * Время: 9:32
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Editor.TextEditors
{
	//TODO 01 подсвечивание парных скобок
	/// <summary>
	/// подсветка парных скобок
	/// </summary>
	public class BracketHighlightRenderer : IBackgroundRenderer
	{
//		BracketSearchResult result;
		Pen borderPen;
		Brush backgroundBrush;
		TextView textView;
		MVVMTextEditor textEditor;
		
		public static readonly Color DefaultBackground = Color.FromArgb(22, 0, 0, 255);
		public static readonly Color DefaultBorder = Color.FromArgb(52, 0, 0, 255);
		
		public const string BracketHighlight = "Bracket highlight";
		
	
		public BracketHighlightRenderer(MVVMTextEditor textEditor)
		{
		
			this.textEditor = textEditor;
			this.textView = textEditor.TextArea.TextView;
			this.textView.BackgroundRenderers.Add(this);
		}

		public void UpdateColors(Color background, Color foreground)
		{
			this.borderPen = new Pen(new SolidColorBrush(foreground), 1);
			this.borderPen.Freeze();

			this.backgroundBrush = new SolidColorBrush(background);
			this.backgroundBrush.Freeze();
		}

		public KnownLayer Layer {
			get {
				return KnownLayer.Selection;
			}
		}

		public static int FindPairBracket(string aText, int aIndex)
		{
			switch(aText[aIndex])
			{
				case '(': //мы стоИм на открывающейся
					return FindPairBracketRight(aText, aIndex);
				case ')': //стоИм на закрывающейся скобке
					return FindPairBracketLeft(aText, aIndex);
			}
			return -1;
		}

		protected static int FindPairBracketLeft(string aText, int aIndex)
		{
			int off = aIndex - 1;
			int level = 1;
			while( off >= 0 )
			{
				switch( aText[off] )
				{
					case ')':
						level++;
						break;
					case '(':
						level--;
						if( level == 0 )
						{
							return off;
						}
						break;
				} 
				off--;
			}

			return 0;
		}

		protected static int FindPairBracketRight(string aText, int aIndex)
		{
			int off = aIndex + 1;
			int level = 1;
			while( off <= aText.Length - 1 )
			{
				switch( aText[off] )
				{
					case '(':
						level++;
						break;
					case ')':
						level--;
						if( level == 0 )
						{
							return off;
						}
						break;
				}
				off++;
			}

			return 0;
		}

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			BackgroundGeometryBuilder builder = new BackgroundGeometryBuilder();

			builder.CornerRadius = 1;
			builder.AlignToMiddleOfPixels = true;

			string str = textEditor.Text;
			if( str == "" )
				return;
			int off1 = textEditor.CaretOffset;
			if( off1 == str.Length) 
				return;

			if( 	str[off1] != '(' 
			   && 	str[off1] != ')'
			   && 	str[off1 - 1] != '(' 
			   && 	str[off1 - 1] != ')')
				return;

			if( off1 > 0 && str[off1 - 1] == '(' )
				off1--;

			int off2 = -1;
			var ch = str.Substring(off1, 1);

			off2 = FindPairBracket(str, off1);
			if( off2 == -1 && str[off1 - 1] == ')')
			{
				off1--;
				off2 = FindPairBracket(str, off1);
			}


			if( off1 != -1)
			{
				builder.AddSegment(textView, new TextSegment() { 
				                   	StartOffset = off1, 
				                   	Length = 1 });
			}

			if( off2 != -1 )
			{
				builder.CloseFigure(); // prevent connecting the two segments
				builder.AddSegment(textView, new TextSegment() { 
				                   	StartOffset = off2, 
				                   	Length = 1 });
			}

			Geometry geometry = builder.CreateGeometry();
			if (geometry != null) {
				drawingContext.DrawGeometry(backgroundBrush, borderPen, geometry);
			}
		}
	}
}
