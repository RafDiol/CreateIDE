   M S B A M L   `   `   `  ����B  >YourIDE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:YourIDEYourIDE  5   	       YourIDE.CreatePrj   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:YourIDE     5   	     � Title$  Create Project��5   	   $	��350��6    $	��500��6-    � 
ResizeMode$ NoResize=�69   .��5	      � .+�5
   
   �� $
��Left=�6   $	��Top=�6+   $��
0, 0, 0, 0q�6C   $��Project Name��6W   �� 5   
   -   5        $��projectNameTextBox��5   r   $
��Left=�6   $	��Top=�6-   $��0, 30, 0, 0q�6E   $	��200��6Z   $��25��6f   6r   �� 5   
   $
��Left=�6   $	��Top=�6+   $��0, 60, 0, 0q�6C   $��Project Path��6X   �� 5   
   -   5        $
��Left=�5      $	��Top=�6,   $��200, 90, 0, 0q�6D   $��25��6[   $��30��6g   .��5      �� $��AppIcons/dotMenu.png��6   5      �� 5   
   -   5        $��projectPathTextBox��5   r   $
��Left=�6   $	��Top=�6-   $��0, 90, 0, 0q�6E   $	��200��6Z   $��25��6f   6r   �� 5   
   $
��Left=�6   $	��Top=�6+   $��0, 120, 0, 0q�6C   $��	Namespace��6Y   �� 5   
   -   5         ��Name$ namespaceTextbox��5      $
��Left=�6,   $	��Top=�6G   $��0, 150, 0, 0q�6_   $	��200��6u   $��25��6�   ��6�   ��  �� Path$
 Text� �� ElementName$ projectNameTextBox�� �� Mode$ OneWay=� �� UpdateSourceTrigger$ PropertyChanged=��� 5   
   -   5        $
��Left=�5      $	��Top=�6,   $��0, 190, 0, 0q�6D   $��Create��6Z   $	��100��6k   6w   5      5      