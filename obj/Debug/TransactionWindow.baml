   M S B A M L   `   `   `  ����  BankManagementSystem,    &BankManagementSystem.TransactionWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Transaction Management��5   	   $	��600��6(   $	��800��65   .��5      � $��10q�6   .+�5   
   �� $��28(�6   $��Center=�6"   $��Transaction Management��6?   $��0,10,0,0q�6]   �� 5   
   $��Account Number:��6   $��	10,70,0,0q�6+   $	��Top=�6>   �� 5   
   -   5        $��AccountNumberTextBox��5      $	��200��6.   $��
150,70,0,0q�6:   $	��Top=�6N   �� 5   
   $��Amount:��6   $��
10,120,0,0q�6#   $	��Top=�67   �� 5   
   -   5        $��AmountTextBox��5      $	��200��6'   $��150,120,0,0q�63   $	��Top=�6H   5   
   �� 5   
   $��Transaction Type:��6   $��
10,170,0,0q�6-   $	��Top=�6A   �� 5   
   -   5        $��TransactionTypeComboBox��5      $	��200��6n   $��150,170,0,0q�6z   $	��Top=�6�   .@�5      �� $��Deposit��6   �� 5      $��Withdraw��6   �� 5      $��Transfer��6   5      �� 5   
   $��Transfer To (Account #):��6   $��
10,220,0,0q�64   $	��Top=�6H   �� 5   
   -   5        $��TransferToTextBox��5      $	��200��6+   $��150,220,0,0q�67   $	��Top=�6L   {�.  6d   �� 5"   
   -   5        $��Process Transaction��5"      $	��200��6/   $��150,280,0,0q�6;   $	��Top=�6P   6h   �� 5%   
   -   5        $��TransactionListView��5%      $��10,350,10,10q�6.    �� View 5&      �� 5'      .P�5(      �� $��Account Number��6%    �� Width$	 150��6l    �� DisplayMemberBinding 6=   �� *AccountNumber+6l   �� 5)      $
��Type��6%   $	 100��6Y    63   �� *Type+6Y   �� 5*      $��Amount��6%   $	 100��6]    65   �� *Amount+6]   �� 5+      $
��Date��6%   $	 200��6Y    63   �� *Date+6Y   5,      5.      50      51      