   M S B A M L   `   `   `  ����  BankManagementSystem1    +BankManagementSystem.AccountStatementWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Account Statement��5   	   $	��600��6#   $	��800��60    � WindowStartupLocation$ CenterScreen=�5   	   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��28(�5      $
��Bold"�5      $��Center=�5      $
��0,10q�5      $��Account Statement��5      �� 5   
   $��1��6    �� Orientation$ 
Horizontal=�5      $��Center=�5      $	��Top=�5      $��	0,30,0,30q�5      .+�5      �� $��Account Number:��6   $��Center=�5       $��14(�5!      $��0,0,10,0q�5"      �� 5#      -   5        $��AccountNumberTextBox��5#      $	��200��5$      $��30��5%      $��0,0,10,0q�5&      �� 5'      -   5        $��Generate Statement��5'      $	��180��5(      $��30��5)      5*      5+      �� 5.   
   -   5        $��TransactionListView��5.       $��2��6   $��0,10,0,0q�6;    �� View 5/      �� 50      .P�51      �� $
��Date��6%    �� Width$	 150��6Y    �� DisplayMemberBinding 63   �� *Date+6Y   �� 52      $
��Type��6%   $	 150��6Y    63   �� *Type+6Y   �� 53      $��Amount��6%   $	 150��6]    65   �� *Amount+6]   �� 54      $��Balance��6%   $	 150��6_    66   �� *	Balance+6_   55      57      58      59      