/*
 * Сделано в SharpDevelop.
 * Пользователь: User
 * Дата: 11.11.2015
 * Время: 10:22
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xaml;
using Editor.ReportDSL;
using Report.Paginator;

namespace RLisp.Preview
{
	/// <summary>
	/// Description of Previewer.
	/// </summary>
	public class Previewer: DocumentViewer
	{
		public Previewer()
		{
			RLisp = new Interpreter();
		}

		private Interpreter RLisp{ set; get;}

		/// <summary>
		/// рендерит отчет
		/// </summary>
		/// <param name="aJson"></param>
		/// <param name="aTemplate"></param>
		/// <param name="aErrorsInPreviewComponent">надо ли засовывать ошибки в предварительный просмотр</param>
		/// <returns></returns>
		public string Preview(string aJson, string aTemplate, bool aErrorsInPreviewComponent = true)
		{
			try
			{
				RLisp.ClearConsole();				
				RLisp.Env = new ReportEnvironment();
				RLisp.SetJSON(aJson);
				RLisp.Eval(aTemplate);

				//TODO нужно запомнить координаты скролов, чтобы постоянно на начало не убегало				
//				var v = this.VerticalOffset;
//				var h = this.HorizontalOffset;
//				var t = this.VisualOffset;
				RenderReport(RLisp.Console, aErrorsInPreviewComponent);				
				var th = new Thread(() => {
			                    	//Thread.Sleep(10);
			                    	Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) delegate(){
			                    	                       	//this.VisualOffset = t;
			                    	                       	//this.VerticalOffset = v;
			                    	                        //this.HorizontalOffset = h;
			                    	                       });
			                    });
				th.Start();

				//this.VerticalOffset = v;
				//this.SetValue(DocumentViewer.VerticalOffsetProperty, v);
				//this.InvalidateProperty(DocumentViewer.VerticalOffsetProperty);
				return RLisp.Console;
			}
			catch(Exception er)
			{
				//вместо отчета выводим отрендеренную ошибку
				if( aErrorsInPreviewComponent == true )
				{
					var i = new Interpreter();
					i.Env = new ReportEnvironment();
					i.ClearConsole();
					i.Eval("(page (:page-padding 2 1 1 1) (par \"" + er.Message + "\"))");
					return RenderReport(i.Console, aErrorsInPreviewComponent);
				}
				return er.Message;
			}
		}

		private string RenderReport(string aText, bool aErrorsInPreview)
		{
			try
			{
				DependencyObject root;

				using(var stream = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(aText)) )
				{
					root = (DependencyObject)XamlServices.Load(stream);
				}

				var def = new PimpedPaginator.Definition(root as FlowDocument);
				var paginator = new PimpedPaginator((FlowDocument)root, def);

				string tempFileName = System.IO.Path.GetTempFileName();
				File.Delete(tempFileName);

				using( XpsDocument xpsDocument = new XpsDocument(tempFileName, FileAccess.ReadWrite) )
				{
					XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
					writer.Write(paginator);
					this.Document = xpsDocument.GetFixedDocumentSequence();
				}
				return aText;
			}catch(Exception e)
			{
				//вместо отчета выводим отрендеренную ошибку
				if( aErrorsInPreview == true )
				{
					var i = new Interpreter();
					i.Env = new ReportEnvironment();
					i.ClearConsole();
					i.Eval("(page (:page-padding 2 1 1 1) (par \"" + e.Message + e.StackTrace + "\"))");
					return RenderReport(i.Console, aErrorsInPreview);
				}
				return e.Message;
			}
		}
	}
}
