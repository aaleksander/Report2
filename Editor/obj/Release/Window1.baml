   M S B A M L   `   `   `  ���� H  DEditor, Version=1.0.5788.22318, Culture=neutral, PublicKeyToken=null7 clr-namespace:Editor.TextEditorsEditor.TextEditors      Editor.Window1   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     1misc clr-namespace:Editor.TextEditors     -     � Title$  Editor��$	��600��$	��900�� � WindowStartupLocation$ CenterScreen=�$��14(�.���  � ColumnDefinitions �� �� $
��Auto ��� .+��� .@��� $��RLisp��.��� $��0�� � RowDefinitions �� $
��0.9* ��� $
��Auto ��� $
��0.1* �.+�'   !Editor.TextEditors.MVVMTextEditor  -     Name$ source��` \ICSharpCode.AvalonEdit, Version=4.4.1.9729, Culture=neutral, PublicKeyToken=9cc39be672370310'  !ICSharpCode.AvalonEdit.TextEditor   ShowLineNumbers . $��14(�$��Consolas)�   SyntaxHighlightingM  GICSharpCode.AvalonEdit.Highlighting.HighlightingDefinitionTypeConverter$	 XML    Text �� *Source+ �� Mode$ TwoWay=�	 �� UpdateSourceTrigger$	 PropertyChanged=��� $��1��$��Center=�$��Stretch=�$��5���� 
 �� 
IsReadOnly
 . $��2������ *Errors+�� $
��Json��.��  -   $
 json�� . $��14(� �� *Json+$ TwoWay=�$	 PropertyChanged=��� $��1��$��5��$��Stretch=�$��Center=��� $��2��.@��� $
��XAML��.����  �� 
AcceptsTab .  �� AcceptsReturn . 
 .  �� TextWrapping$
 Wrap=����� *	Console+$ TwoWay=��� $��Просмотр��.��]� ��. .+��� -   $	��Top=�$��5q�$	��100��$��Посмотреть��Z� -   $��preview��