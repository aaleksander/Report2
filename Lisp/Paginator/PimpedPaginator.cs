using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.IO;

namespace Report.Paginator //Tetra.Framework.WPF
{
	public static class VisualTreeHelperExt
	{
		public static T FindAncestor<T>(DependencyObject depend) where T: class
		{
			DependencyObject target = depend;
			do
			{
				target = VisualTreeHelper.GetParent(target);
			}
			while( target != null && !(target is T) );
			return target as T;
		}
	}

	/// <summary>
	/// This paginator provides document headers, footers and repeating table headers 
	/// </summary>
	/// <remarks>
	/// </remarks>
	public class PimpedPaginator : DocumentPaginator {

		public PimpedPaginator(FlowDocument document, Definition def) {
			// Create a copy of the flow document,
			// so we can modify it without modifying
			// the original.
			_doc = document;
			MemoryStream stream = new MemoryStream();
			TextRange sourceDocument = new TextRange(document.ContentStart, document.ContentEnd);
			sourceDocument.Save(stream, DataFormats.Xaml);
			FlowDocument copy = new FlowDocument();
			TextRange copyDocumentRange = new TextRange(copy.ContentStart, copy.ContentEnd);
			copyDocumentRange.Load(stream, DataFormats.Xaml);
			this.paginator = ((IDocumentPaginatorSource)copy).DocumentPaginator;
			this.definition = def;
			paginator.PageSize = def.ContentSize;

			// Change page size of the document to
			// the size of the content area
			copy.ColumnWidth = double.MaxValue; // Prevent columns
			copy.PageWidth = definition.ContentSize.Width;
			copy.PageHeight = definition.ContentSize.Height;
			copy.PagePadding = new Thickness(0);
		}

		private FlowDocument _doc;
		private DocumentPaginator paginator;
		private Definition definition;

		private void DumpVisualTree(int aPageNumber, DependencyObject parent, int level)
		{
			//var tt = VisualTreeHelper.GetVisualType(parent);
			string tt = "test";
			string indent = "";
			for(int j=0; j<level; j++)
				indent += "\t";
			bool needDeep = true; //надо ли погружаться глубже

			Type t = parent.GetType();
			string typeName = t.Name;
			string name = (string)parent.GetValue(FrameworkElement.NameProperty);
			string val = "";
			var dbProp = t.GetProperty("DescendantBounds");

			if( t.Name == "SectionVisual") 
			{
				var realPar1 = VisualTreeHelper.GetParent(parent);
				var realPar2 = VisualTreeHelper.GetParent(realPar1);
				var pr1 = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			}

			val = " ----- ";
			string text = "";

			if( parent.IsPage() )
			{
				typeName = "Страница: " + parent.IsPageEndTable().ToString();
			}

			if( parent.IsText() )
			{
				typeName = "текст";
				text = parent.GetText();
				needDeep = false; //дальше погружаться смысла нет
			}

			if( parent.IsTable() )
			{
				typeName = "таблица";
			}

			if( parent.IsRow() )
			{
				typeName = "Строка таблицы";
			}

			if( parent.IsCell() )
			{
				typeName = "ячейка таблицы";
			}
			
			if( parent.IsEmpty() )
			{
				typeName = "пусто";
			}

//			if( t.Name == "LineVisual" )
//			{
//				try
//				{
//					var ttt = ((parent as DrawingVisual).Drawing.Children[0] as DrawingGroup).Children[0] as GlyphRunDrawing;
//					foreach(var ch in ttt.GlyphRun.Characters)
//						text += ch;
//				}catch(Exception)
//				{
//					text = "empty";
//				}
////				val = ttt.GlyphRun.Characters.ToString();
//			}

			if( dbProp != null ) //есть размеры
			{
				Rect obj = (Rect)dbProp.GetValue(parent, null);				
				val = (Math.Floor(obj.Left)).ToString();
				val += "; " + (Math.Floor(obj.Top)).ToString() + " - ";
				val += (Math.Floor(obj.Width)).ToString() + " x " + (Math.Floor(obj.Height)).ToString();
			}

			using(StreamWriter file = new StreamWriter("page_" + aPageNumber.ToString() + ".txt", true))
			{
				
				file.WriteLine(string.Format(@"{2}{0}:{1} = {3}, {5}, ""{4}""", typeName, name, indent, val, text, tt));
			}

			if( parent == null || needDeep == false )
				return;
			for(int i=0; i<VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				//DumpVisualTree(aPageNumber, child, level + 1);
			}
		}


		public override DocumentPage GetPage(int pageNumber) {
			// Use default paginator to handle pagination
			Visual originalPage = paginator.GetPage(pageNumber).Visual;

			//DumpVisualTree(pageNumber, originalPage, 0);

			ContainerVisual visual = new ContainerVisual();
			ContainerVisual pageVisual = new ContainerVisual() {
				Transform = new TranslateTransform(
					definition.ContentOrigin.X, 
					definition.ContentOrigin.Y
				)
			};
			pageVisual.Children.Add(originalPage);
			visual.Children.Add(pageVisual);

			// если есть заголовок или подвал, то вставляем
			if(definition.Header != null) {
				visual.Children.Add(CreateHeaderFooterVisual(definition.Header, definition.HeaderRect, pageNumber));
			}
			if(definition.Footer != null) {
				visual.Children.Add(CreateHeaderFooterVisual(definition.Footer, definition.FooterRect, pageNumber));
			}

			// Если есть сквозные строки
			if(definition.RepeatTableHeaders) {
				// находим заголовок таблицы
				ContainerVisual table;
				if( PageStartsWithTable(originalPage, out table) && currentHeader != null) {
					// Таблица начинается с таблицы и у нас уже есть сквозные строки c предыдущей страницы
					// Presumably this table 
					// was started on the previous page, so we'll repeat the
					// table header.
					Rect headerBounds = VisualTreeHelper.GetDescendantBounds(currentHeader);
					Vector offset = VisualTreeHelper.GetOffset(currentHeader);
					ContainerVisual tableHeaderVisual = new ContainerVisual();

					// Translate the header to be at the top of the page
					// instead of its previous position
					tableHeaderVisual.Transform = new TranslateTransform(
						definition.ContentOrigin.X,
						definition.ContentOrigin.Y - headerBounds.Top
					);

					// Since we've placed the repeated table header on top of the
					// content area, we'll need to scale down the rest of the content
					// to accomodate this. Since the table header is relatively small,
					// this probably is barely noticeable.
					double yScale = (definition.ContentSize.Height - headerBounds.Height) / definition.ContentSize.Height;
					TransformGroup group = new TransformGroup();
					group.Children.Add(new ScaleTransform(1.0, yScale));
					group.Children.Add(new TranslateTransform(
						definition.ContentOrigin.X,
						definition.ContentOrigin.Y + headerBounds.Height
					));
					pageVisual.Transform = group;

					ContainerVisual cp = VisualTreeHelper.GetParent(currentHeader) as ContainerVisual;
					if(cp != null) {
						cp.Children.Remove(currentHeader);
					}
					tableHeaderVisual.Children.Add(currentHeader);
					visual.Children.Add(tableHeaderVisual);
				}

				// Check if there is a table on the bottom of the page.
				// If it's there, its header should be repeated
				ContainerVisual newTable, newHeader;
				if(PageEndsWithTable(originalPage, out newTable, out newHeader)) 
				{
					if(newTable == table)
					{
						// Still the same table so don't change the repeating header
					} else 
					{
						// We've found a new table. Repeat the header on the next page
						currentHeader = newHeader;
						//table = newTable;
					}
				} 
				else {
					// There was no table at the end of the page
					currentHeader = null;
				}
			}

			return new DocumentPage(
				visual, 
				definition.PageSize, 
				new Rect(new Point(), definition.PageSize),
				new Rect(definition.ContentOrigin, definition.ContentSize)
			);
		}

		/// <summary>
		/// Creates a visual to draw the header/footer
		/// </summary>
		/// <param name="draw"></param>
		/// <param name="bounds"></param>
		/// <param name="pageNumber"></param>
		/// <returns></returns>
		private Visual CreateHeaderFooterVisual(DrawHeaderFooter draw, Rect bounds, int pageNumber) {
			DrawingVisual visual = new DrawingVisual();
			using(DrawingContext context = visual.RenderOpen()) {
				draw(context, bounds, pageNumber);
			}
			return visual;
		}

		ContainerVisual currentHeader = null;

		private object GetPrivateProperty(object aObj, string aPropertyName)
		{
			PropertyInfo pInfo = aObj.GetType().GetProperty(aPropertyName,
															   BindingFlags.Public |                                                   
			                                                   BindingFlags.NonPublic |
			                                                   BindingFlags.Instance
			                                                  );
			if( pInfo == null )
				return null;
			return pInfo.GetValue(aObj, null);
		}

		private bool CanBeThrough(ContainerVisual a)
		{
			var row = (TableRow)GetPrivateProperty(a, "Row");
			if( row == null )
				return false;

			foreach(var c in row.Cells)
			{
				if( c.Blocks.Count != 1 ) //какая-то сложная ячейка, не подходит
				{
					return false;
				}

				if( c.Blocks.FirstBlock is Paragraph )
				{
					Paragraph par = (Paragraph)c.Blocks.FirstBlock;
					//склеиваем все инлайны в один текс
					StringBuilder sb = new StringBuilder();
					foreach(var i in par.Inlines)
					{
						if( i is Run )
						{
							sb.Append( (i as Run).Text );
						}
					}
					string txt = sb.ToString();
					try
					{
						int ttt = int.Parse(txt);
					}catch(FormatException)  //то не INT
					{
						return false;
					}					
				}
				else
				{ //это не параграф и фиг с ним
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// ищет у таблицы строку, пригодную на роль сквозной
		/// стока считается сквозной, если количество ячеек в ней равно количеству столбцов
		/// и содержит только целочисленный значения.
		/// </summary>
		/// <param name="aTable"></param>
		/// <returns></returns>
		private int GetThroughIndex(ContainerVisual aTable)
		{
			for(int i = 0; i< aTable.Children.Count; i++)
			{
				if( CanBeThrough((ContainerVisual)aTable.Children[i]) )
				{
					return i;
				}
			}
			//не нашли ни одной строки
			return 0;
		}

		/// <summary>
		/// Checks if the page ends with a table.
		/// </summary>
		/// <remarks>
		/// There is no such thing as a 'TableVisual'. There is a RowVisual, which
		/// is contained in a ParagraphVisual if it's part of a table. For our
		/// purposes, we'll consider this the table Visual
		/// 
		/// You'd think that if the last element on the page was a table row, 
		/// this would also be the last element in the visual tree, but this is not true
		/// The page ends with a ContainerVisual which is aparrently  empty.
		/// Therefore, this method will only check the last child of an element
		/// unless this is a ContainerVisual
		/// </remarks>
		/// <param name="originalPage"></param>
		/// <returns></returns>
		private bool PageEndsWithTable(DependencyObject element, 
		                               out ContainerVisual tableVisual, 
		                               out ContainerVisual headerVisual) 
		{
			tableVisual = null;
			headerVisual = null;

			var b1 = element.IsPage();
			var b2 = element.First().Last().IsEmpty();

			var last = element.First().First().First().First().Last();
			
			if( last.IsTable() == false ) //СТРаница заканчивается не таблицей
			{
				return false;
			}
			else
			{
				var throughIndex = GetThroughIndex(last as ContainerVisual);
				tableVisual = last as ContainerVisual;
				headerVisual = (ContainerVisual)VisualTreeHelper.GetChild(last, throughIndex);
				return true;
			}
/*
			Type t = element.GetType();
			string typeName = t.Name;
			string name = (string)element.GetValue(FrameworkElement.NameProperty);

			tableVisual = null;
			headerVisual = null;
			if( element.IsRow() ) { //это строка
				tableVisual = (ContainerVisual)VisualTreeHelper.GetParent(element);
//				var tmp = VisualTreeHelperExt.FindAncestor<Table>(element);

				//берем индекс сквозной строки
				var throughIndex = GetThroughIndex(tableVisual);

//				var ch = VisualTreeHelper.GetChild(element, 0).GetType();
//				var t = tableVisual.GetType();//.Ge(Table.TagProperty);
//				var l = tableVisual.GetLocalValueEnumerator();
				//var f = VisualTreeHelper.GetChild(table
				//var tt = tableVisual.GetType().GetProperties(BindingFlags.Public);// GetValue(.TagProperty);
				headerVisual = (ContainerVisual)VisualTreeHelper.GetChild(tableVisual, throughIndex);
				return true;
			}
			int children = VisualTreeHelper.GetChildrenCount(element);
			if(element.GetType() == typeof(ContainerVisual)) {
				for(int c = children - 1; c >= 0; c--) {
					DependencyObject child = VisualTreeHelper.GetChild(element, c);
					if(PageEndsWithTable(child, out tableVisual, out headerVisual)) {
						return true;
					}
				}
			} else if(children > 0) {
				DependencyObject child = VisualTreeHelper.GetChild(element, children - 1);
				if(PageEndsWithTable(child, out tableVisual, out headerVisual)) {
					return true;
				}
			}
			return false;*/
		}


		/// <summary>
		/// Checks if the page starts with a table which presumably has wrapped
		/// from the previous page.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="tableVisual"></param>
		/// <param name="headerVisual"></param>
		/// <returns></returns>
		private bool PageStartsWithTable(DependencyObject element, out ContainerVisual tableVisual) {
			tableVisual = null;
			if(element.GetType().Name == "RowVisual") {
				tableVisual = (ContainerVisual)VisualTreeHelper.GetParent(element);
				return true;
			}
			if(VisualTreeHelper.GetChildrenCount(element)> 0) {
				DependencyObject child = VisualTreeHelper.GetChild(element, 0);
				if(PageStartsWithTable(child, out tableVisual)) {
					return true;
				}
			}
			return false;
		}


		#region DocumentPaginator members

		public override bool IsPageCountValid {
			get { return paginator.IsPageCountValid; }
		}

		public override int PageCount {
			get { return paginator.PageCount; }
		}

		public override Size PageSize {
			get {
				return paginator.PageSize;
			}
			set {
				paginator.PageSize = value;
			}
		}

		public override IDocumentPaginatorSource Source {
			get { return paginator.Source; }
		}

		#endregion


		public class Definition 
		{

			public Definition(FlowDocument aDoc)
			{
				PageSize = new Size(aDoc.PageWidth, aDoc.PageHeight);
				Margins = aDoc.PagePadding;
			}

			#region Page sizes
			/// <summary>
			/// PageSize in DIUs
			/// </summary>
			public Size PageSize {
				get { return _PageSize; }
				set { _PageSize = value; }
			}
			private Size _PageSize = new Size(793.5987, 1122.3987); // Default: A4

			/// <summary>
			/// Margins
			/// </summary>
			public Thickness Margins {
				get { return _Margins; }
				set { _Margins = value; }
			}
			private Thickness _Margins = new Thickness(96); // Default: 1" margins


			/// <summary>
			/// Space reserved for the header in DIUs
			/// </summary>
			public double HeaderHeight {
				get { return _HeaderHeight; }
				set { _HeaderHeight = value; }
			}
			private double _HeaderHeight;

			/// <summary>
			/// Space reserved for the footer in DIUs
			/// </summary>
			public double FooterHeight {
				get { return _FooterHeight; }
				set { _FooterHeight = value; }
			}
			private double _FooterHeight;

			#endregion


			public DrawHeaderFooter Header, Footer;

			///<summary>
			/// Should table headers automatically repeat?
			///</summary>
			public bool RepeatTableHeaders {
				get { return _RepeatTableHeaders; }
				set { _RepeatTableHeaders = value; }
			}
			private bool _RepeatTableHeaders = true;


			#region Some convenient helper properties

			internal Size ContentSize {
				get {
					Size res = new Size(PageSize.Width - (Margins.Left + Margins.Right)
					                    ,PageSize.Height - (Margins.Top + Margins.Bottom + HeaderHeight + FooterHeight));
					return res;
//					return PageSize.Subtract(new Size(Margins.Left + Margins.Right,
//						Margins.Top + Margins.Bottom + HeaderHeight + FooterHeight
//					));
				}
			}

			internal Point ContentOrigin {
				get {
					return new Point(
						Margins.Left,
						Margins.Top + HeaderRect.Height
					);
				}
			}

			internal Rect HeaderRect {
				get {
					return new Rect(
						Margins.Left, Margins.Top,
						ContentSize.Width, HeaderHeight
					);
				}
			}

			internal Rect FooterRect {
				get {
					return new Rect(
						Margins.Left, ContentOrigin.Y + ContentSize.Height,
						ContentSize.Width, FooterHeight
					);
				}
			}

			#endregion

		}

		/// <summary>
		/// Allows drawing headers and footers
		/// </summary>
		/// <param name="context">This is the drawing context that should be used</param>
		/// <param name="bounds">The bounds of the header. You can ignore these at your own peril</param>
		/// <param name="pageNr">The page nr (0-based)</param>
		public delegate void DrawHeaderFooter(DrawingContext context, Rect bounds, int pageNr);

	}
}


//{Num: "",PR_Type: "Faure Herman HELIFLU TZN250-2000",
//	PR_ManufNum: "9901",
//	PR_Owner: "АО \"Транснефть-Прикамье\"",
//	OilType: "товарная нефть",PU_Type: "СФРЮ-4000-25-40",
//	PU_ManufNum: "7280",Visc_Init: "0",Visc_Last: "0", ts:1444315415865,"ViscMin":0.0,"ViscMax":0.0,
//	"VolWaterFrac":0.0,
//"Table1":{"m_DetNum":[131,242],"m_Vo":[40.036,40.032],"m_D":882.6,"m_S":7.0,"m_E":206700.0,
//		"m_alpha":0.0000112,"m_theta_sum":0.05,"m_theta_V0":0.019,"m_theta_SOI":0.024,"m_delta_tpu":0.2,
//		"m_delta_tpr":0.2,"m_Ro":0.0,"m_tRo":0.0,"m_tst":0.0},	
//"Table2":[{"m_num_series":1,"m_num_meas":1,"m_Q":0.0,"m_DetNum":131,"m_T":2.911,"m_f":100.0,"m_N":291.146,
//	          	"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          	"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474,
//	          	"m_gamma_g":2.696539702293474,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474,
//	          	"m_kpg":2.696539702293474,"m_ktp":2.696539702293474},
//	          	{"m_num_series":1,"m_num_meas":2,"m_Q":0.0,"m_DetNum":131,"m_T":3.781,"m_f":100.0,"m_N":378.12,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,
//	          		"m_tRo":0.0,"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":3,"m_Q":0.0,"m_DetNum":131,"m_T":4.034,"m_f":100.0,"m_N":403.445,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":4,"m_Q":0.0,"m_DetNum":242,"m_T":4.376,"m_f":100.0,"m_N":437.61,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":5,"m_Q":0.0,"m_DetNum":131,"m_T":3.624,"m_f":100.0,"m_N":362.361,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":6,"m_Q":0.0,"m_DetNum":242,"m_T":3.87,"m_f":100.0,"m_N":387.037,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":1,"m_Q":0.0,"m_DetNum":131,"m_T":3.311,"m_f":99.999,"m_N":331.047,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":2,"m_Q":0.0,"m_DetNum":242,"m_T":3.48,"m_f":100.0,"m_N":347.967,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":3,"m_Q":0.0,"m_DetNum":131,"m_T":2.476,"m_f":100.0,"m_N":247.574,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":4,"m_Q":0.0,"m_DetNum":242,"m_T":2.725,"m_f":99.999,"m_N":272.488,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":5,"m_Q":0.0,"m_DetNum":131,"m_T":2.882,"m_f":99.999,"m_N":288.185,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":6,"m_Q":0.0,"m_DetNum":242,"m_T":2.977,"m_f":100.0,"m_N":297.708,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":1,"m_Q":0.0,"m_DetNum":131,"m_T":2.316,"m_f":100.0,"m_N":231.555,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":2,"m_Q":0.0,"m_DetNum":242,"m_T":2.762,"m_f":99.998,"m_N":276.165,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":3,"m_Q":0.0,"m_DetNum":131,"m_T":1.954,"m_f":99.999,"m_N":195.405,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":4,"m_Q":0.0,"m_DetNum":242,"m_T":2.4,"m_f":100.0,"m_N":239.968,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":5,"m_Q":0.0,"m_DetNum":131,"m_T":2.446,"m_f":100.0,"m_N":244.569,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":6,"m_Q":0.0,"m_DetNum":242,"m_T":3.106,"m_f":100.0,"m_N":310.58,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308}],
//	"Table2_Qty":18,
//	"Table3":[
//		{"m_num_series":1,"m_Q":0.0,"m_f":0.0,"m_K":0.0,"m_S":0.0,"m_eps":0.0,"m_theta_sum":0.0,"m_delta":0.0},
//		{"m_num_series":2,"m_Q":0.0,"m_f":0.0,"m_K":0.0,"m_S":0.0,"m_eps":0.0,"m_theta_sum":0.0,"m_delta":0.0},
//		{"m_num_series":3,"m_Q":0.0,"m_f":0.0,"m_K":0.0,"m_S":0.0,"m_eps":0.0,"m_theta_sum":0.0,"m_delta":0.0}],
//	"Table3_Qty":3,
//	"Table4":[
//		{"m_num_subrange":1,"m_Qmin":0.0,"m_Qmax":0.0,"m_eps":0.0,"m_theta_a":0.0,"m_theta_sum":0.0,"m_delta":0.0,
//			"m_K":0.0}],
//	"Table4_Qty":1,
//	"Table5":{"m_Qmin":0.0,"m_Qmax":0.0,"m_Eps":0.0,"m_Thetaa":0.0,"m_Thetasum":0.0,"m_Delta":0.0,"m_K":0.0},
//	"Table6":[],
//	"Table6_Qty":0,
//	"RndWay1974":{
//		"TempRnd":1,"PressRnd":2,"ViscRnd":1,"HumRnd":1,"DensRnd":1,"FlowRateRnd":1,"GrossRateRnd":0,"VolRnd":5,
//		"KoefRnd":3,"DeltaRnd":3,"KtpRnd":6,"ImpQtyRnd":3,"TimeRnd":3,"FreqRnd":3,"FreqViscRnd":3,"Betta":6,"Gamma":6
//	}
//}


//{Num: "",PR_Type: "Faure Herman HELIFLU TZN250-2000",
//	PR_ManufNum: "9901",
//	PR_Owner: "АО \"Транснефть-Прикамье\"",
//	OilType: "товарная нефть",PU_Type: "СФРЮ-4000-25-40",
//	PU_ManufNum: "7280",Visc_Init: "0",Visc_Last: "0", ts:1444315415865,"ViscMin":0.0,"ViscMax":0.0,
//	"VolWaterFrac":0.0,
//"Table1":{"m_DetNum":[131,242],"m_Vo":[40.036,40.032],"m_D":882.6,"m_S":7.0,"m_E":206700.0,
//		"m_alpha":0.0000112,"m_theta_sum":0.05,"m_theta_V0":0.019,"m_theta_SOI":0.024,"m_delta_tpu":0.2,
//		"m_delta_tpr":0.2,"m_Ro":0.0,"m_tRo":0.0,"m_tst":0.0},	
//"Table2":[{"m_num_series":1,"m_num_meas":1,"m_Q":0.0,"m_DetNum":131,"m_T":2.911,"m_f":100.0,"m_N":291.146,
//	          	"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          	"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474,
//	          	"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474,
//	          	"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":2,"m_Q":0.0,"m_DetNum":131,"m_T":3.781,"m_f":100.0,"m_N":378.12,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,
//	          		"m_tRo":0.0,"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":3,"m_Q":0.0,"m_DetNum":131,"m_T":4.034,"m_f":100.0,"m_N":403.445,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":4,"m_Q":0.0,"m_DetNum":242,"m_T":4.376,"m_f":100.0,"m_N":437.61,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":5,"m_Q":0.0,"m_DetNum":131,"m_T":3.624,"m_f":100.0,"m_N":362.361,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":1,"m_num_meas":6,"m_Q":0.0,"m_DetNum":242,"m_T":3.87,"m_f":100.0,"m_N":387.037,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":1,"m_Q":0.0,"m_DetNum":131,"m_T":3.311,"m_f":99.999,"m_N":331.047,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":2,"m_Q":0.0,"m_DetNum":242,"m_T":3.48,"m_f":100.0,"m_N":347.967,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":3,"m_Q":0.0,"m_DetNum":131,"m_T":2.476,"m_f":100.0,"m_N":247.574,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":4,"m_Q":0.0,"m_DetNum":242,"m_T":2.725,"m_f":99.999,"m_N":272.488,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":5,"m_Q":0.0,"m_DetNum":131,"m_T":2.882,"m_f":99.999,"m_N":288.185,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":2,"m_num_meas":6,"m_Q":0.0,"m_DetNum":242,"m_T":2.977,"m_f":100.0,"m_N":297.708,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":1,"m_Q":0.0,"m_DetNum":131,"m_T":2.316,"m_f":100.0,"m_N":231.555,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":2,"m_Q":0.0,"m_DetNum":242,"m_T":2.762,"m_f":99.998,"m_N":276.165,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":3,"m_Q":0.0,"m_DetNum":131,"m_T":1.954,"m_f":99.999,"m_N":195.405,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":4,"m_Q":0.0,"m_DetNum":242,"m_T":2.4,"m_f":100.0,"m_N":239.968,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":5,"m_Q":0.0,"m_DetNum":131,"m_T":2.446,"m_f":100.0,"m_N":244.569,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308},
//	          	{"m_num_series":3,"m_num_meas":6,"m_Q":0.0,"m_DetNum":242,"m_T":3.106,"m_f":100.0,"m_N":310.58,
//	          		"m_K":0.0,"m_Tpr":17.9,"m_Ppr":0.71,"m_Tpu":0.0,"m_Ppu":0.0,"m_tst":0.0,"m_Ro":0.0,"m_tRo":0.0,
//	          		"m_v":0.0,"m_Vo":0.0,"m_P_Ro":0.0,"m_Dens15":0.0,"m_betta_g":2.696539702293474e308,
//	          		"m_gamma_g":2.696539702293474e308,"m_kt":0.999328,"m_kp":1.0,"m_ktg":2.696539702293474e308,
//	          		"m_kpg":2.696539702293474e308,"m_ktp":2.696539702293474e308}],
//	"Table2_Qty":18,
//	"Table3":[
//		{"m_num_series":1,"m_Q":0.0,"m_f":0.0,"m_K":0.0,"m_S":0.0,"m_eps":0.0,"m_theta_sum":0.0,"m_delta":0.0},
//		{"m_num_series":2,"m_Q":0.0,"m_f":0.0,"m_K":0.0,"m_S":0.0,"m_eps":0.0,"m_theta_sum":0.0,"m_delta":0.0},
//		{"m_num_series":3,"m_Q":0.0,"m_f":0.0,"m_K":0.0,"m_S":0.0,"m_eps":0.0,"m_theta_sum":0.0,"m_delta":0.0}],
//	"Table3_Qty":3,
//	"Table4":[
//		{"m_num_subrange":1,"m_Qmin":0.0,"m_Qmax":0.0,"m_eps":0.0,"m_theta_a":0.0,"m_theta_sum":0.0,"m_delta":0.0,
//			"m_K":0.0}],
//	"Table4_Qty":1,
//	"Table5":{"m_Qmin":0.0,"m_Qmax":0.0,"m_Eps":0.0,"m_Thetaa":0.0,"m_Thetasum":0.0,"m_Delta":0.0,"m_K":0.0},
//	"Table6":[],
//	"Table6_Qty":0,
//	"RndWay1974":{
//		"TempRnd":1,"PressRnd":2,"ViscRnd":1,"HumRnd":1,"DensRnd":1,"FlowRateRnd":1,"GrossRateRnd":0,"VolRnd":5,
//		"KoefRnd":3,"DeltaRnd":3,"KtpRnd":6,"ImpQtyRnd":3,"TimeRnd":3,"FreqRnd":3,"FreqViscRnd":3,"Betta":6,"Gamma":6
//	}
//}