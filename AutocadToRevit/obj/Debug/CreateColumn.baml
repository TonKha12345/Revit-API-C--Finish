   M S B A M L   `   `   `  ����T  PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  5      I EAutocadToRevit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:AutocadToRevit.LibAutocadToRevit.Lib 5      %   AutocadToRevit.Lib.CreateColumn   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      Imah;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:AutocadToRevit.Lib     5        � Title$  Create Column��5	       � WindowStartupLocation$ CenterScreen=�5
       � 
ResizeMode$ NoResize=�5      $	��287��6#   $	��627��60    � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $I Cpack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml?�6%   �� 5      $F @pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml?�6%   �� 5      $R Lpack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Blue.xaml?�6%   %    5      .��5      ��  �� Orientation$ Vertical=�6   $��10,10,10,10q�6(   $	��NaN��6=   $	��NaN��6I   .+�5   
   � $	��199��6    � ColumnDefinitions 5      �� 5      $	��27* �6#   61   �� 5      $	��49* �6#   61   �� 5      $	��38* �6#   61   �� 5       $	��38* �6#   61    � RowDefinitions 5"      �� 5#      6"   �� 5$      6"   �� 5%      6"   �� 5&      6"   .+�5)      �� $��Layer��6   $��15(�6%   $��Center=�5*      $
��Left=�63   $��20��6N   $��35��6Z   5,      �� 5.      $��1��6   $��Column Family��6%   $��15(�6:   $��Center=�5/      $
��Left=�63   $��20��6N   $	��102��6Z   51      �� 53      $��2��6   $��
Base Level��6%   $��15(�67   $��Center=�54      $
��Left=�63   $��20��6N   $	��102��6Z   56      �� 58      $��3��6   $��	Top Level��6%   $��15(�66   $��Center=�59      $
��Left=�63   $��20��6N   $	��102��6Z   5;      �� 5=      $��2��6   $��2��6%   $��Base Offset��65   $��15(�6H   $��Center=�5>      $��Center=�63   $��20��6P   $	��102��6\   5@      �� 5B      $��3��6   $��2��6%   $��
Top Offset��65   $��15(�6G   $��Center=�5C      $��Center=�63   $��20��6P   $	��102��6\   5E      �� 5G      $��1��6   $��3��6'   $	��NaN��6;   $��	0,10,0,10q�6H   	 �� 
IsEditable	 . ��5H      �� *	AllLayers+
 �� SelectedItem
 5I      �� *SelectedLayer+5J      �� 5L      $��1��6   $��1��6'   $��3��64   $	��NaN��6H   $��	0,10,0,10q�6U    �� DisplayMemberPath$
 Name��5O      	 . ��5M      �� *AllFamiliesColumn+
 5N      �� *SelectedFamilyColumn+5P      �� 5R      $��1��6   $��2��6'   $	��NaN��65   $��	0,10,0,10q�6B   $
 Name��5U      	 . ��5S      �� *
AllLevel+
 5T      �� *	BaseLevel+5V      �� 5X      $��1��6   $��3��6'   $	��NaN��65   $��	0,10,0,10q�6B   $
 Name��5[      	 . ��5Y      �� *
AllLevel+
 5Z      �� *
TopLevel+5\      �� 5^      $��3��6   $��2��6&   $��	0,10,0,10q�63    � ToolTip$ The unit is milimeter��5`      ��5_      �� *
BaseOffset+5`   8   �� 5b      $��3��6   $��3��6&   $��	0,10,0,10q�63   $ The unit is milimeter��5d      ��5c      �� *	TopOffset+5d   8   5e      ]� 5g   
   ��. $��39��6)   .+�5h      �� -   5        $��Right=�5h      $��Cancle��6,   $��55��6=   $��30��6H   
����  �$��15(�5j   (    '  #MahApps.Styles.Button.Square.Accent#��[  5k      5m      �� 5o      -   5        $��Right=�5o      $��Ok��6,   $��61��69   $��30��6D   
����  �$��5,0,5,0q�5p   '   $��15(�5q   $   #��[  5r      5s      � 5u      .+�5v      � -   5        $��ProgressWindow��5v   )   $��5q�6   6A   �� 5x      $��2��6   $��Center=�5z      $��Center=�5{      $��15(�67   ��5y      �� *	Percent+ �� StringFormat$ {0:0}%��5{   7   5|      5}      5~      5      