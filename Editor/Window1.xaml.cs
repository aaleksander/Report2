using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xaml;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Report.Paginator;
using ViewModel;

//RLisp

//TODO цикл while, until

//TODO IsEmptyContainer //это притащилось из паджинатора

//TODO комментарии. многострочные

//TODO если в json задать несуществующий путь, то будет непонятное исключение

//TODO json-exists? - существует ли какой-то ключ (включая поля в цикле)


//генерация отчетов

//TODO отделение данных, как в лиспе, символом ' , чтобы можно было писать: (par 'бла-бла-бла, изоляицяия: \), пока не встречу)

//TODO вставка картинок (img (:margin ...) (:border ... ) (:width ...) (:height ...) "image.png")

//TODO работа с цветом (foreground и background) по принципу superscript ...

//TODO курсив

//TODO колонтитулы: верхние, нижние

//TODO pad-left pad-right pad - дополнить строку пробелами до определенной длины

//TODO генерация списков через точки, (list 1 ... 3) или (list 1 3 ... 100)


namespace Editor
{

	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
			DataContext = new MainViewModel();

			source.ReloadHighLighting(); //Надо как-то избавиться от этого вызова
		}

		double v = 0;
		double h = 0;
		void Button_Click(object sender, RoutedEventArgs e)
		{
			//TODO убрать сохранение/восстановление offset внутрь библиотеки
			v = preview.VerticalOffset;
			h = preview.HorizontalOffset;
			xaml.Text = preview.Preview(json.Text, source.Text);
			var th = new Thread(() => {
			                    	//Thread.Sleep(10);
			                    	Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) delegate(){
										preview.VerticalOffset = v;
										preview.HorizontalOffset = h;
									});
			                    });
			th.Start();
		}

		void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			File.WriteAllText("data.json", json.Text);
			File.WriteAllText("Report.txt", source.Text);
		}

//		void Button_Click1(object sender, RoutedEventArgs e)
//		{
//			preview.VerticalOffset = v;
//			preview.HorizontalOffset = h;
//		}
	}
}