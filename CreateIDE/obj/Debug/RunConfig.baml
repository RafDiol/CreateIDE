   M S B A M L   `   `   `  ����B  >YourIDE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:CreateIDE	CreateIDE  5   	       CreateIDE.RunConfig   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:CreateIDE     5   	     � Title$  Run Settings��5   	   $	��500��6   $	��450��6+    � 
ResizeMode$ NoResize=�67   .��5	      � .+�5
   
   �� .��5      �� .+�5      �� $��Type of output��6   �� 5      -   5         ��Name$ outputTypeComboBox��5      $	��Top=�67   $
��Left=�6O   $	��100��6j   $��
0, 0, 0, 0q�6v   .@�5      �� $	��EXE��6#   �� 5      $	��DLL��6#   5      �� 5      -   5        $	��Top=�5   2   $
��Left=�6J   $	��150��6e   $��0, 10, 0, 0q�6q   $��Change Source File��6�    � ToolTip$ The startup object��6�   �� 5      $	��Top=�6   $��20��60   $
��Left=�6<   $	��150��6W   $��0, 10, 0, 0q�6c   �� 5      -   5         ��Name$ autoRunCheckBox��5      $	��Top=�64   $
��Left=�6L   $��0, 20, 0, 0q�6g   $>��8Auto run after compiling (Only works for the EXE format)��6|   �� 5      $	��Top=�6   $
��Left=�60   $��0, 30, 0, 0q�6K   $��
References��6`   �� 5      -   5         ��Name$ 
refListBox��5      $	��Top=�6.   $
��Left=�6F   $	��200��6a   $	��150��6m   $��0, 10, 0, 0q�6z   $E ?If a DLL can't be found without a direct reference add one here��6�   5      �� 5      -   5        $	��Add��5      $	��Top=�66   $
��Left=�6N   $	��100��6i   $��25��6u   $��200, -105, 0, 0q�6�   �� 5      -   5        $��Remove��5      $	��Top=�6<   $
��Left=�6T   $	��100��6o   $��25��6{   $��200, -75, 0, 0q�6�   �� 5      $	��Top=�6   $
��Left=�60   $	��200��6K   $��25��6W   $��0, 15, 0, 0q�6c   $��Warning Level��6x   �� 5      -   5        $ warningLvlComboBox��5      $	��Top=�67   $
��Left=�6O   $	��200��6j   $��25��6v   $��0, 25, 0, 0q�6�   .@�5      �� $��1��6#   �� 5      $��2��6#   �� 5      $��3��6#   �� 5       $��4��6#   �� 5!      $��5��6#   5"      �� 5#      -   5        $# treatWarningsAsErrorsCheckbox��5#      $��Treat Warnings As Errors��6B   $	��Top=�6e   $
��Left=�6}   $	��200��6�   $��25��6�   $��0, 30, 0, 0q�6�   �� 5$      -	   5        $ inculdeDebugInfoCheckbox��5$      $��Include Debug Info��6=   $	��Top=�6Z   $
��Left=�6r   $	��200��6�   $��25��6�   $��0, 30, 0, 0q�6�   �� 5%      -
   5        $��Bottom=�5%      $��Stretch=�64   $��35��6R   $��Apply��6^   6n   5&      5'      5(      5)      