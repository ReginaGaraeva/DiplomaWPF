   M S B A M L   `   `   `  ����[  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms  5   	   ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35tOclr-namespace:System.Windows.Forms.Integration;assembly=WindowsFormsIntegration System.Windows.Forms.Integration 5   	   m iSystem.Windows.Forms.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35�mclr-namespace:System.Windows.Forms.DataVisualization.Charting;assembly=System.Windows.Forms.DataVisualization/System.Windows.Forms.DataVisualization.Charting 5   	    FIeldDataAnalyzer"   FIeldDataAnalyzer.Statistics   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     5   	   _wfiOclr-namespace:System.Windows.Forms.Integration;assembly=WindowsFormsIntegration     5   	   }dvcmclr-namespace:System.Windows.Forms.DataVisualization.Charting;assembly=System.Windows.Forms.DataVisualization     5   	   -   5          � Title$  Статистика��5   	   $��490.202��6   $	��963��6-   .��5      � .+�5	   
   ��  �� TabStripPlacement$
 Left=�6   .@�5
      �� $��Скважины��6   .��5      � ���	#FFE5E5E5.+�5      �� .@�5      �� $C��=Статистика измерений по скважине��6"   .��5      � ���	#FFE5E5E5 � RowDefinitions 5   "   �� 5   &   $	��20* �64   �� 5   &   $
��161* �64   .+�5   "   � $��1��6(    � ColumnDefinitions 5   &   �� 5   *   $
��582* �6;   �� 5   *   $
��209* �6;   .+�5   &   7  1System.Windows.Forms.Integration.WindowsFormsHost     Child. 5   *   ;  5System.Windows.Forms.DataVisualization.Charting.Chart  -   5        	wellChart��5   4   5   '   �� 5   &   $��Отображать:��6,   $
��Left=�6B   $��	20,26,0,0q�6]   $��1��6p   $	��Top=�6�   �� 5   &   -   5         ��Name$ 
T1CheckBox��5   /   $5��/Температура до штуцера (T1)��6C   $
��Left=�6i   $��	20,72,0,0q�6�   $��1��6�   $	��Top=�6�    d� 	IsChecked$
 TrueY�6�   6�   �� 5   &   -   5        $ 
T2CheckBox��5   /   $;��5Температура после штуцера (T2)��6C   $
��Left=�6l   $��
20,100,0,0q�6�   $��1��6�   $	��Top=�6�   6�   �� 5   &   -   5        $ 
P1CheckBox��5   /   $/��)Давление до штуцера (P1)��6C   $
��Left=�6f   $��
20,130,0,0q�6�   $��1��6�   $	��Top=�6�   6�   �� 5    &   -   5        $ 
P2CheckBox��5    /   $5��/Давление после штуцера (P2)��6C   $
��Left=�6i   $��
20,160,0,0q�6�   $��1��6�   $	��Top=�6�   6�   �� 5!   &   -   5        $ 	GCheckBox��5!   /   $'��!Массовый расход (G)��6B   $
��Left=�6`   $��
20,190,0,0q�6{   $��1��6�   $	��Top=�6�   6�   5"   #   � 5#   "    5$   &   �� 5%   *   $
��344* �6;   �� 5&   *   $
��447* �6;   .+�5(   &   �� $'��!Выберите скважину��6,   $
��Left=�6H   $��10,0,0,0q�6c   $��Center=�6u   $	��119��6�   �� 5)   &   -   5         ��Name$ wellsComboBox��5)   /   $
��Left=�6F   $��151,12,0,13q�6a   $��Center=�6v   $	��147��6�   6�   5*   #   �� 5+   "   $��Даты с��6(   $
��Left=�69   $��419,10,0,11q�6T   $��Center=�6i   (  "System.Windows.Controls.DatePicker  5,   "   $
��Left=�6-   $��471,11,0,12q�6H   $��Center=�6]   $	��126��6x   �� 5-   "   $
��по��6(   $
��Left=�65   $��597,10,0,11q�6P   $��Center=�6e     5.   "   $
��Left=�6-   $��627,11,0,12q�6H   $��Center=�6]   $	��126��6x   �� 5/   "   $��?��6)   $
��Left=�65   $��314,13,0,14q�6P   $��Center=�6e   $��20��6�   �� 50   "   $��Все данные��6+   $
��Left=�6@   $��
761,16,0,0q�6[   $	��Top=�6o    J� RenderTransformOrigin$ 0.565,0.822&�6�   52      53      �� 54      $M��GСравнительная статистика по скважинам��6"   .��55      � ���	#FFE5E5E56#   56      57      58      59      �� 5:      $��УКПГ��6   .��5;      � ���	#FFE5E5E56   5<      �� 5=      $��Оптимизация��6   .��5>      � ���	#FFE5E5E56   5?      5@      5B      5C      