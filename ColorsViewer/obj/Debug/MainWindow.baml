   M S B A M L   `   `   `  ����H  DNumericUpDown, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null@0clr-namespace:CAVControls;assembly=NumericUpDownCAVControls  5   	    ColorsViewer   ColorsViewer.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   CcavCtl0clr-namespace:CAVControls;assembly=NumericUpDown     5   	     � Title$0  *Colors Viewer: System.Windows.Media.Colors��5   	   $	��592��6<   $	��744��6I    � Icon$3 -icons/Iconshock-Brilliant-Graphics-Colors.ico��6U   
���   �
�������.��5      �  � RowDefinitions 5	   
   �� 5
      $
��Auto �6   �� 5      .+�5   
   �� $��0 0 0 2q�6   
�������._�5      � $��5q�6   .+�5      ��  �� Orientation$ 
Horizontal=�6   $��	0,0,20,10q�66   .+�5      �� $/��)Ширина прямоугольника��6    $��80��6=   $
��Wrap=�6H   $��10(�6\   $��Center=�6j      CAVControls.NumericUpDown  5      -   5          Name$ nudWidth��5   +      Minimum$ 10~�6=      Maximum$	 200~�6J      Value$ 50~�6X   $	��100��6c      FontSize$ 20Q�6o   $
��Bold"�6}   	   IsVisibleRange$	 Visible=�6�   5      �� 5      $ 
Horizontal=�6   $��	0,0,20,10q�66   .+�5      �� $/��)Высота прямоугольника��6    $��80��6=   $
��Wrap=�6H   $��10(�6\     5      -   5        $ 	nudHeight��5   +   $ 8~�6>   $	 150~�6J   $ 30~�6X   $	��100��6c   $ 20Q�6o   $
��Bold"�6}   $	 Visible=�6�   5      �� 5      $��	0,0,20,10q�6   ._�5      �� -   5        
 ��Name$
 rbColors��5   S   $ Vertical=�6!   $��Center=�68   .+�5      	� -   5         	�Name$ 	rbtColors��5   &    d� IsThreeState .  .��6m   System.Windows.Media.Colors6�   	� 5       -   5        $ rbtSystemColors��5    &    .  .��6r   System.Windows.SystemColors6�   5!      5"      �� 5%      $ 
Horizontal=�6   $��	0,0,20,10q�66   .+�5&      �� $��Цвет текста: ��6    $��Center=�65   �� 5'      -   5         ��Name$ listForeColors��5'      $��Center=�67   $	��100��6R   6^   5(      �� 5+      $ 
Horizontal=�6   $��	0,0,20,10q�66   .+�5,      �� $��Цвет фона: ��6    $��Center=�63   �� 5-      -   5        $ listBackColors��5-      $��Center=�67   $	��100��6R   6^   5.      �� 51      $ 
Horizontal=�6   $��
20,0,20,10q�66   .+�52      �� -   5         ��Name$ btnCopyToClibboard��52      $��copy to clipboard��69   $��8,0,8,3q�6V    � ToolTip$� |скопировать в буфер обмена строку в формате [цвет текста]|[цвет фона]��6h   6�   �� 53      $��10,0,0,0q�6!   .+�54      	� -	   5        $ rbtToClipByName��54   &   $��by Name��6?    .  6Q   	� 55      -
   5        $ rbtToClipByRGB��55   &   $��by RGB��6>    .  6O   56      57      58      59      �� 5;   
   $��1��6   .��5<      � -   5         �Name$ 	wrapPanel��5<       � ContextMenu 5=      �� 5>       �� HasDropShadow . .@�5?      m� -   5        $��as Foreground��5?   #   6:   m� 5@      -   5        $��as Background��5@   #   6:   5A      5C      5D      5E      5F      