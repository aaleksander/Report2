using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Editor.TextEditors
{
	/// <summary>
	/// подпиливает TextEditor в соответствие с MVVM-паттерном.
	/// Плюс сворачивание текста (folding)
	/// Плюс моя подсветка синтаксиса
	/// </summary>
	public class MVVMTextEditor: TextEditor, INotifyPropertyChanged
	{
		public MVVMTextEditor():base()
		{

		}

		#region MVVM	
		public static DependencyProperty CaretOffsetProperty =
			DependencyProperty.Register("CarretOffset", typeof(int), typeof(MVVMTextEditor),
			                            new PropertyMetadata((obj, args) =>
			                                                 {
			                                                 	MVVMTextEditor target = (MVVMTextEditor)obj;
			                                                 	target.CaretOffset = (int)args.NewValue;
			                                                 }));
		public static DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(MVVMTextEditor),
			                            new PropertyMetadata((obj, args) =>
			                                                 {
			                                                 	MVVMTextEditor target = (MVVMTextEditor)obj;
			                                                 	target.Text = (string)args.NewValue;
			                                                 }));
		public new string Text
		{
			get{
				return base.Text;
			}
			set{
				base.Text = value;
				RaisePropertyChanged("Text");
			}
		}

		public new int CaretOffset{
			get{
				return base.CaretOffset;
			}
			set{
				base.CaretOffset = value;
			}
		}

		public int Length{
			get{
				return base.Text.Length;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			RaisePropertyChanged("Text");
			base.OnTextChanged(e);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void RaisePropertyChanged(string info)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion
	}
}
