using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;


namespace Report.Paginator
{
	/// <summary>
	/// расширения для DependencyObjecta
	/// </summary>
	public static class DependencyObjectExt
	{
		public static bool IsPage(this DependencyObject element)
		{			
			return element.GetType().Name == "PageVisual";
		}
		
		public static bool IsRow(this DependencyObject element)
		{			
			return element.GetType().Name == "RowVisual";
		}

		public static bool IsParagraph(this DependencyObject element)
		{			
			return element.GetType().Name == "ParagraphVisual";
		}

		public static bool IsLine(this DependencyObject element)
		{
			return element.GetType().Name == "LineVisual";
		}

		public static bool IsContainer(this DependencyObject element)
		{
			return element.GetType().Name == "ContainerVisual";
		}
		
		public static bool IsSection(this DependencyObject element)
		{
			return element.GetType().Name == "SectionVisual";
		}

		public static bool IsTable(this DependencyObject element)
		{
			
			var ll = new List<Func<DependencyObject, bool>>(){
				x => x.IsParagraph(),								//ParagraphVisual: = 63; 142 - 36 x 12, test, ""
				x => x.ChildCount() > 0,							//у ячейки два потомка: текст и конец конструкции
				x => x.First().IsRow(),						//	ContainerVisual: = 62; 3 - 36 x 12, test, ""
			};
			
			return CheckListFunction(element, ll);
		}

		public static bool IsPageEndTable(this DependencyObject element)
		{
//			var b1 = element.IsPage();
//			var b2 = element.First().Last().IsEmpty();

			var last = element.First().First().First().First().Last();

			return last.IsTable();
		}

		/// <summary>
		/// является ли этот элемент ячейкой таблицы
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static bool IsCell(this DependencyObject element)
		{
//			ParagraphVisual: = 63; 142 - 36 x 12, test, ""
//				ContainerVisual: = 62; 3 - 36 x 12, test, ""
//					SectionVisual: = 62; 3 - 36 x 12, test, ""
//						ContainerVisual: = 62; 3 - 36 x 12, test, ""
//							текст: = 62; 3 - 36 x 12, test, "Сдал:"
//				ContainerVisual: = бесконечность; бесконечность - -бесконечность x -бесконечность, test, ""
			var ll = new List<Func<DependencyObject, bool>>(){
				x => x.IsParagraph(),								//ParagraphVisual: = 63; 142 - 36 x 12, test, ""
				x => x.ChildCount() == 2,							//у ячейки два потомка: текст и конец конструкции
				x => x.First().IsContainer(),						//	ContainerVisual: = 62; 3 - 36 x 12, test, ""
				x => x.First().First().IsSection(),					//		SectionVisual: = 62; 3 - 36 x 12, test, ""
				x => x.First().First().First().IsContainer(),		//			ContainerVisual: = 62; 3 - 36 x 12, test, ""
				x => x.First().First().First().First().IsText()					
			};

			return CheckListFunction(element, ll);

//			if( element.IsParagraph() == false ) return false;
//			if( element.ChildCount() != 2 ) return false;
//			if( element.First().IsContainer() == false ) return false;
//			var t1 = element.First();
//			var	t2 = element.First().First();
//			if( element.First().First().IsSection() == false ) return false;
//			if( element.First().First().First().IsContainer() == false) return false;
//			if( element.First().First().First().First().IsText() == false ) return false;
//			return true;
		}


		public static bool IsEmpty(this DependencyObject element)
		{
			var ll = new List<Func<DependencyObject, bool>>(){
				x => x.IsContainer(),
				x => x.ChildCount() == 0
			};

			return CheckListFunction(element, ll); 
		}

		private static bool CheckListFunction(DependencyObject aObj, List<Func<DependencyObject, bool>> aList)
		{
			try
			{
				foreach(var f in aList)
				{
					if( f(aObj) == false )
					{
						return false;
					}
				}
				return true;
			}catch(Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// обычный параграф с текстом?
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static bool IsText(this DependencyObject element)
		{//par->par(1)->lines
			if( !element.IsParagraph() )
			{
				return false;
			}

			if( element.ChildCount() != 1 )
				return false;

			if( !element.First().IsParagraph() )
				return false;

			if( !element.First().First().IsLine() )
				return false;

			return true;
		}

		public static string GetText(this DependencyObject element)
		{
			var parent = element.First().First();
			if( ((parent as DrawingVisual).Drawing.Children[0] as DrawingGroup).Children.Count == 0 )
				return "empty";
			var ttt = ((parent as DrawingVisual).Drawing.Children[0] as DrawingGroup).Children[0] as GlyphRunDrawing;
			string text = "";
			
			try
			{
				foreach(var ch in ttt.GlyphRun.Characters)
					text += ch;
			}catch(Exception)
			{
				text = "exception!!!";
			}
			return text;
		}

		public static int ChildCount(this DependencyObject element)
		{
			var b1 = element is Drawing;
			var b2 = element is ContainerVisual;
			
			return (element as ContainerVisual).Children.Count;
		}

		public static DependencyObject First(this DependencyObject element)
		{
			try
			{
				return (element as DrawingVisual).Children[0];
			}catch(NullReferenceException )
			{
				return (element as ContainerVisual).Children[0];
			}
		}
		
		/// <summary>
		/// последний элемент
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static DependencyObject Last(this DependencyObject element)
		{
			DrawingVisual dv;
			ContainerVisual cv;
			try
			{
				dv = element as DrawingVisual;
				return dv.Children[dv.Children.Count - 1];
			}catch(NullReferenceException )
			{
				cv = element as ContainerVisual;
				return cv.Children[cv.Children.Count - 1];
			}
		}
		
		/// <summary>
		/// предпоследний элемент
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static DependencyObject PreLast(this DependencyObject element)
		{
			DrawingVisual dv;
			ContainerVisual cv;
			try
			{
				dv = element as DrawingVisual;
				return dv.Children[dv.Children.Count - 2];
			}catch(NullReferenceException )
			{
				cv = element as ContainerVisual;
				return cv.Children[cv.Children.Count - 2];
			}
		}
	}
}
