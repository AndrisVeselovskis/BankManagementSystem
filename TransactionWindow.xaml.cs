using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Input;

namespace BankManagementSystem
{
    public partial class TransactionWindow : Window
    {
        private int loggedInUserId;

        public TransactionWindow(int userId)
        {
            InitializeComponent();
            loggedInUserId = userId;
            LoadTransactions();
            TransactionTypeComboBox.SelectionChanged += TransactionTypeComboBox_SelectionChanged;
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string input = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            input = input.Replace(',', '.');

            if (!Regex.IsMatch(input, @"^\d*(\.\d{0,2})?$"))
            {
                e.Handled = true;
            }
        }

        private void AmountTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void AmountTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasted = ((string)e.DataObject.GetData(typeof(string))).Replace(',', '.');

                if (!Regex.IsMatch(pasted, @"^\d+(\.\d{1,2})?$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TransactionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransactionTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();
                TransferToTextBox.IsEnabled = selectedValue == "Transfer";
            }
            else
            {
                TransferToTextBox.IsEnabled = false;
            }
        }

        private void LoadTransactions()
        {
            TransactionListView.Items.Clear();

            using (var connection = DatabaseHelper.GetConnection())
            {
                string query;

                if (DatabaseHelper.IsUserAdmin(loggedInUserId))
                {
                    // Admin sees all transactions
                    query = "SELECT AccountNumber, Type, Amount, Date FROM Transactions ORDER BY Date DESC";
                }
                else
                {
                    // Regular user sees only their own account's transactions
                    query = @"
                SELECT AccountNumber, Type, Amount, Date 
                FROM Transactions 
                WHERE AccountNumber = (
                    SELECT AccountNumber FROM Users WHERE UserId = @UserID
                ) 
                ORDER BY Date DESC";
                }

                using (var command = new SQLiteCommand(query, connection))
                {
                    if (!DatabaseHelper.IsUserAdmin(loggedInUserId))
                        command.Parameters.AddWithValue("@UserID", loggedInUserId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TransactionListView.Items.Add(new
                            {
                                AccountNumber = reader["AccountNumber"],
                                Type = reader["Type"],
                                Amount = reader["Amount"],
                                Date = reader["Date"]
                            });
                        }
                    }
                }
            }
        }

        private void ProcessTransaction_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = AccountNumberTextBox.Text;
            if (!DatabaseHelper.IsUserAccountOwner(loggedInUserId, accountNumber))
            {
                MessageBox.Show("Access denied! You can only manage your own account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string input = AmountTextBox.Text.Replace(',', '.');

            if (!Regex.IsMatch(input, @"^\d+(\.\d{0,2})?$"))
            {
                MessageBox.Show("Amount must be a number with up to 2 decimal places.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount greater than 0.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string transactionType = ((ComboBoxItem)TransactionTypeComboBox.SelectedItem).Content.ToString();
            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();
                if (transactionType == "Transfer")
                {
                    string transferTo = TransferToTextBox.Text.Trim();

                    // Check if target account exists
                    string checkTargetQuery = "SELECT COUNT(1) FROM Accounts WHERE AccountNumber = @Target";
                    using (var checkCmd = new SQLiteCommand(checkTargetQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Target", transferTo);
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            MessageBox.Show("Target account does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Get sender's current balance
                    decimal senderBalance = 0;
                    string balanceQuery = "SELECT Balance FROM Accounts WHERE AccountNumber = @AccountNumber";
                    using (var balanceCmd = new SQLiteCommand(balanceQuery, connection))
                    {
                        balanceCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        senderBalance = Convert.ToDecimal(balanceCmd.ExecuteScalar());
                    }

                    // Check if it's a credit card and get current balance
                    bool isCreditCard = false;
                    decimal currentBalance = 0;
                    string typeQuery = "SELECT Balance, CardType FROM Accounts A LEFT JOIN Cards C ON A.AccountNumber = C.AccountNumber WHERE A.AccountNumber = @AccountNumber";
                    using (var typeCmd = new SQLiteCommand(typeQuery, connection))
                    {
                        typeCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        using (var reader = typeCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentBalance = Convert.ToDecimal(reader["Balance"]);
                                isCreditCard = reader["CardType"].ToString() == "Credit";
                            }
                        }
                    }

                    // Enforce overdraft rule
                    if (isCreditCard && (currentBalance - amount) < -2000)
                    {
                        MessageBox.Show("Credit card cannot go below -2000.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // For regular accounts (not credit), no overdraft allowed
                    if (!isCreditCard && (currentBalance - amount) < 0)
                    {
                        MessageBox.Show("Insufficient balance. Cannot overdraft.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Proceed with transfer
                    ExecuteUpdate(connection, accountNumber, -amount);
                    ExecuteUpdate(connection, transferTo, amount);
                    SaveTransaction(connection, accountNumber, "Transfer", -amount);
                    SaveTransaction(connection, transferTo, "Transfer", amount);
                }
                else if (transactionType == "Withdraw")
                {
                    decimal totalDeposits = 0;
                    decimal totalWithdrawals = 0;

                    using (var checkCommand = new SQLiteCommand(@"
                        SELECT 
                        SUM(CASE WHEN Type = 'Deposit' THEN Amount ELSE 0 END) AS TotalDeposits,
                        SUM(CASE WHEN Type = 'Withdraw' THEN Amount ELSE 0 END) AS TotalWithdrawals
                            FROM Transactions
                            WHERE AccountNumber = @AccountNumber", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);

                        using (var reader = checkCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                totalDeposits = reader["TotalDeposits"] != DBNull.Value ? Convert.ToDecimal(reader["TotalDeposits"]) : 0;
                                totalWithdrawals = reader["TotalWithdrawals"] != DBNull.Value ? Convert.ToDecimal(reader["TotalWithdrawals"]) : 0;
                            }
                        }
                    }

                    if (totalDeposits == 0)
                    {
                        MessageBox.Show("Withdrawals are not allowed without a prior deposit.", "Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if ((totalWithdrawals + amount) > totalDeposits)
                    {
                        MessageBox.Show("Withdrawal exceeds total deposits. Operation not allowed.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Proceed with withdrawal
                    ExecuteUpdate(connection, accountNumber, amount);
                    SaveTransaction(connection, accountNumber, "Withdraw", amount);
                }
                else if (transactionType == "Deposit")
                {
                    // Check if account exists
                    string checkQuery = "SELECT COUNT(1) FROM Accounts WHERE AccountNumber = @AccountNumber";
                    using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        long exists = (long)checkCmd.ExecuteScalar();
                        if (exists == 0)
                        {
                            MessageBox.Show("Account does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    ExecuteUpdate(connection, accountNumber, -amount);
                    SaveTransaction(connection, accountNumber, "Deposit", amount);
                }
            }
            MessageBox.Show("Transaction successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTransactions();
            ClearTransactionFields();
        }

        private void ExecuteUpdate(SQLiteConnection connection, string accountNumber, decimal amount)
        {
            string query = "UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountNumber = @AccountNumber";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.ExecuteNonQuery();
            }
        }

        private void SaveTransaction(SQLiteConnection connection, string accountNumber, string type, decimal amount)
        {
            string query = "INSERT INTO Transactions (AccountNumber, Type, Amount, Date) VALUES (@AccountNumber, @Type, @Amount, @Date)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@Type", type);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@Date", DateTime.Now);
                command.ExecuteNonQuery();
            }
        }
        private void ClearTransactionFields()
        {
            AccountNumberTextBox.Text = "";
            AmountTextBox.Text = "";
            TransferToTextBox.Text = "";

            if (TransactionTypeComboBox.SelectedIndex >= 0)
                TransactionTypeComboBox.SelectedIndex = -1;

            TransferToTextBox.IsEnabled = false;
            AccountNumberTextBox.Focus();
        }
    }
}
