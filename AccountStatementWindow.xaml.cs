using System;
using System.Data.SQLite;
using System.Windows;
using System.Collections.Generic;

namespace BankManagementSystem
{
    public partial class AccountStatementWindow : Window
    {
        private int loggedInUserId;

        public AccountStatementWindow(int userId)
        {
            InitializeComponent();
            loggedInUserId = userId;
        }

        private void GenerateStatement_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = AccountNumberTextBox.Text.Trim();

            if (!DatabaseHelper.IsUserAdmin(loggedInUserId) &&
                !DatabaseHelper.IsUserAccountOwner(loggedInUserId, accountNumber))
            {
                MessageBox.Show("Access denied! You can only view statements for your own accounts.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(accountNumber))
            {
                MessageBox.Show("Please enter an account number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadAccountStatement(accountNumber);
        }

        private void LoadAccountStatement(string accountNumber)
        {
            TransactionListView.Items.Clear();
            string dbFile = "bank.db";

            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();

                // Fetch current balance and basic info
                string infoQuery = @"
            SELECT FirstName, LastName, Balance 
            FROM Accounts 
            WHERE AccountNumber = @AccountNumber";

                decimal currentBalance = 0;
                string firstName = "", lastName = "";

                using (var command = new SQLiteCommand(infoQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        firstName = reader["FirstName"].ToString();
                        lastName = reader["LastName"].ToString();
                        currentBalance = Convert.ToDecimal(reader["Balance"]);
                    }
                }

                // Fetch transactions ordered oldest → newest
                var transactions = new List<(string Type, decimal Amount, DateTime Date)>();

                string transactionQuery = @"
            SELECT Type, Amount, Date
            FROM Transactions 
            WHERE AccountNumber = @AccountNumber 
            ORDER BY Date ASC";

                using (var cmd = new SQLiteCommand(transactionQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string type = reader["Type"].ToString();
                            decimal amount = Convert.ToDecimal(reader["Amount"]);
                            DateTime date = Convert.ToDateTime(reader["Date"]);
                            transactions.Add((type, amount, date));
                        }
                    }
                }

                // Now build running balance backwards
                // Start from current and reverse apply transactions
                transactions.Reverse();
                var runningBalances = new List<decimal>();
                decimal balance = currentBalance;

                foreach (var tx in transactions)
                {
                    switch (tx.Type)
                    {
                        case "Deposit":
                            balance -= tx.Amount;
                            break;
                        case "Withdraw":
                        case "LoanPayment":
                            balance += tx.Amount;
                            break;
                        case "Transfer":
                            balance -= tx.Amount;
                            break;
                        case "LoanApproved":
                            balance -= tx.Amount;
                            break;
                        default:
                            // Treat unrecognized types as outgoing by default
                            balance -= tx.Amount;
                            break;
                    }
                    runningBalances.Add(balance);
                }

                // Reverse again to get original order and forward balance
                transactions.Reverse();
                runningBalances.Reverse();

                for (int i = transactions.Count - 1; i >= 0; i--)
                {
                    var tx = transactions[i];
                    TransactionListView.Items.Add(new
                    {
                        Date = tx.Date.ToString("dd.MM.yyyy HH:mm"),
                        Type = tx.Type,
                        Amount = tx.Amount,
                        Balance = runningBalances[i].ToString("C")
                    });
                }

                MessageBox.Show($"Statement for {firstName} {lastName}", "Account Statement", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
