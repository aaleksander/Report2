   M S B A M L   `   `   `  ����G  CRLisp, Version=1.0.5807.30271, Culture=neutral, PublicKeyToken=nullD.clr-namespace:RLisp.TextEditors;assembly=RLispRLisp.TextEditors  5      <*clr-namespace:RLisp.Preview;assembly=RLispRLisp.Preview  5      
 Editor   Editor.Window1   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @rlisp.clr-namespace:RLisp.TextEditors;assembly=RLisp     5      >preview*clr-namespace:RLisp.Preview;assembly=RLisp     5      -   5          � Title$  Editor��5      $	��600��6   $	��900��6    � WindowStartupLocation$ CenterScreen=�5      $��14(�5      .��5      �  � ColumnDefinitions 5      �� 5      �� 5      $
��Auto �6   �� 5      .+�5      �� .@�5      �� $��RLisp��6   .��5      � $��0��6    � RowDefinitions 5      �� 5   	   $
��0.9* �6   �� 5   	   $
��Auto �6   �� 5   	   $
��0.1* �6   .+�5      &    RLisp.TextEditors.TemplateEditor  -   5          Name$ source��5   4   ` \ICSharpCode.AvalonEdit, Version=4.4.1.9729, Culture=neutral, PublicKeyToken=9cc39be672370310'  !ICSharpCode.AvalonEdit.TextEditor   ShowLineNumbers . $��14(�5      $��Consolas)�6      SyntaxHighlightingM  GICSharpCode.AvalonEdit.Highlighting.HighlightingDefinitionTypeConverter$	 XML 5      &    RLisp.TextEditors.MVVMTextEditor   Text 5      �� *Source+ �� Mode$ TwoWay=�	 �� UpdateSourceTrigger$	 PropertyChanged=��� 5       $��1��6   $��Center=�6"   $��Stretch=�6=   $��5��6[   �� 5!      
 �� 
IsReadOnly
 . $��2��6"   ��6/   �� *Errors+5"      5#      �� 5%      $
��Json��6   .��5&        -   5          Name$
 json��5&   3    . $��14(�5'   
    5(      �� *Json+$ TwoWay=�$	 PropertyChanged=�5*      5+      �� 5,      $��1��6   $��5��6!   $��Stretch=�6+   $��Center=�6G   �� 5.      $��2��6   .@�5/      �� $
��XAML��6   .��50      �� -   5        $
��xaml��50       �� 
AcceptsTab .  �� AcceptsReturn . 
 .  �� TextWrapping$
 Wrap=�51      53      �� 54      $��Просмотр��6   .��55      ]� ��. .+�56      �� -   5        $	��Top=�56      $��5q�6#   $	��100��6.   $��Посмотреть��6:   6O      RLisp.Preview.Previewer  57      -   5        $��preview��57      58      59      5:      5;      5<      