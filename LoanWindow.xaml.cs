using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BankManagementSystem
{
    public partial class LoanWindow : Window
    {
        private readonly string userRole;

        public class Loan
        {
            public int LoanID { get; set; }
            public string AccountNumber { get; set; }
            public string LoanType { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; }
            public decimal RemainingAmount { get; set; }
        }

        private ObservableCollection<Loan> loans = new ObservableCollection<Loan>();
        private List<Loan> approvedLoans = new List<Loan>();

        public LoanWindow(string role)
        {
            InitializeComponent();
            userRole = role;
            PendingLoansListView.ItemsSource = loans;
            LoadPendingLoans();
            LoadApprovedLoans();
            RestrictLoanPermissions();
        }

        private void RestrictLoanPermissions()
        {
            if (userRole == "Klient")
            {
                ApproveLoanButton.Visibility = Visibility.Collapsed;
                RejectLoanButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LoanAmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Allow only digits and 1 decimal point
            if (!Regex.IsMatch(fullText, @"^\d*\.?\d{0,2}$"))
            {
                e.Handled = true;
            }
        }

        private void ApplyLoan_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = AccountNumberTextBox.Text.Trim();
            string loanType = ((ComboBoxItem)LoanTypeComboBox.SelectedItem)?.Content.ToString();
            string amountText = LoanAmountTextBox.Text.Replace(',', '.');

            if (!decimal.TryParse(LoanAmountTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Invalid loan amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            amount = Math.Round(amount, 2);

            if (decimal.Round(amount, 2) != decimal.Parse(LoanAmountTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture))
            {
                MessageBox.Show("Amount was rounded to 2 decimal places.", "Note", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                // 🔍 Check if account exists
                string checkAccountQuery = "SELECT COUNT(1) FROM Accounts WHERE AccountNumber = @AccountNumber";
                using (var checkCmd = new SQLiteCommand(checkAccountQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("This account number does not exist in the database.", "Invalid Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                string insertLoanQuery = "INSERT INTO Loans (AccountNumber, LoanType, Amount, Status) VALUES (@AccountNumber, @LoanType, @Amount, 'Pending')";
                using (var insertCmd = new SQLiteCommand(insertLoanQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    insertCmd.Parameters.AddWithValue("@LoanType", loanType);
                    insertCmd.Parameters.AddWithValue("@Amount", amount);
                    insertCmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Loan application submitted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadPendingLoans();
            LoadApprovedLoans();
        }

        private void LoadPendingLoans()
        {
            loans.Clear();

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                // First: Load only PENDING loans for approval window
                string pendingQuery = "SELECT LoanID, AccountNumber, LoanType, Amount, Status FROM Loans WHERE Status = 'Pending'";

                using (var pendingCommand = new SQLiteCommand(pendingQuery, connection))
                using (var reader = pendingCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        loans.Add(new Loan
                        {
                            LoanID = Convert.ToInt32(reader["LoanID"]),
                            AccountNumber = reader["AccountNumber"].ToString(),
                            LoanType = reader["LoanType"].ToString(),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Status = reader["Status"].ToString()
                        });
                    }
                }
            }

            PendingLoansListView.ItemsSource = null;
            PendingLoansListView.ItemsSource = loans;
        }

        private void LoadApprovedLoans()
        {
            approvedLoans.Clear(); // Make sure you have a List<Loan> approvedLoans = new List<Loan>();

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                string approvedQuery = "SELECT LoanID, AccountNumber, LoanType, Amount, RemainingAmount, Status FROM Loans WHERE Status = 'Approved'";

                using (var approvedCommand = new SQLiteCommand(approvedQuery, connection))
                using (var reader = approvedCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        approvedLoans.Add(new Loan
                        {
                            LoanID = Convert.ToInt32(reader["LoanID"]),
                            AccountNumber = reader["AccountNumber"].ToString(),
                            LoanType = reader["LoanType"].ToString(),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            RemainingAmount = reader["RemainingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["RemainingAmount"]) : 0,
                            Status = reader["Status"].ToString()
                        });
                    }
                }
            }

            ApprovedLoansListView.ItemsSource = null;
            ApprovedLoansListView.ItemsSource = approvedLoans;
        }

        private void PayLoanButton_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = AccountNumberTextBox.Text.Trim();
            if (string.IsNullOrEmpty(accountNumber))
            {
                MessageBox.Show("Please enter a valid account number.");
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                // Get unpaid total
                decimal totalUnpaid = 0;
                using (var cmd = new SQLiteCommand("SELECT SUM(RemainingAmount) FROM Loans WHERE AccountNumber = @AccountNumber AND Status = 'Approved'", connection))
                {
                    cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    var result = cmd.ExecuteScalar();
                    totalUnpaid = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }

                if (totalUnpaid <= 0)
                {
                    MessageBox.Show("No unpaid loans found.");
                    return;
                }

                // Open payment dialog
                var dialog = new PayLoanDialog(totalUnpaid);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    decimal payment = dialog.SelectedAmount;

                    // Check balance
                    decimal balance = 0;
                    using (var cmd = new SQLiteCommand("SELECT Balance FROM Accounts WHERE AccountNumber = @AccountNumber", connection))
                    {
                        cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        var result = cmd.ExecuteScalar();
                        balance = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }

                    if (balance < payment)
                    {
                        // Get card type
                        string cardType = "None";
                        using (var typeCmd = new SQLiteCommand("SELECT IFNULL(C.CardType, 'None') AS CardType FROM Cards C WHERE AccountNumber = @AccountNumber", connection))
                        {
                            typeCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            var result = typeCmd.ExecuteScalar();
                            cardType = result?.ToString() ?? "None";
                        }

                        // Calculate projected balance after payment
                        decimal projectedBalance = balance - payment;

                        // Apply credit card rules
                        if (cardType == "Credit")
                        {
                            if (projectedBalance < -2000)
                            {
                                MessageBox.Show("Payment would exceed the credit limit of -2000.", "Credit Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        else
                        {
                            if (projectedBalance < 0)
                            {
                                MessageBox.Show("Insufficient balance for loan payment.", "Insufficient Funds", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        // Deduct from account
                        using (var cmd = new SQLiteCommand("UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountNumber = @AccountNumber", connection))
                        {
                            cmd.Parameters.AddWithValue("@Amount", payment);
                            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            cmd.ExecuteNonQuery();
                        }

                        // Apply payment across loans
                        using (var cmd = new SQLiteCommand("SELECT LoanID, RemainingAmount FROM Loans WHERE AccountNumber = @AccountNumber AND Status = 'Approved' ORDER BY LoanID", connection))
                        {
                            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read() && payment > 0)
                                {
                                    int loanId = Convert.ToInt32(reader["LoanID"]);
                                    decimal remaining = Convert.ToDecimal(reader["RemainingAmount"]);
                                    decimal toPay = Math.Min(remaining, payment);
                                    payment -= toPay;

                                    // Update RemainingAmount
                                    using (var updateCmd = new SQLiteCommand("UPDATE Loans SET RemainingAmount = RemainingAmount - @Amount WHERE LoanID = @LoanID", connection))
                                    {
                                        updateCmd.Parameters.AddWithValue("@Amount", toPay);
                                        updateCmd.Parameters.AddWithValue("@LoanID", loanId);
                                        updateCmd.ExecuteNonQuery();
                                    }

                                    // Mark as paid if fully paid
                                    if (toPay == remaining)
                                    {
                                        using (var statusCmd = new SQLiteCommand("UPDATE Loans SET Status = 'Paid' WHERE LoanID = @LoanID", connection))
                                        {
                                            statusCmd.Parameters.AddWithValue("@LoanID", loanId);
                                            statusCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }

                        Loan selectedLoan = approvedLoans.FirstOrDefault(l => l.AccountNumber == accountNumber);
                        // Record transaction
                        using (var cmd = new SQLiteCommand("INSERT INTO Transactions (AccountNumber, Type, Amount, Date) VALUES (@AccountNumber, 'LoanPayment', @Amount, @Date)", connection))
                        {
                            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            cmd.Parameters.AddWithValue("@LoanID", selectedLoan.LoanID);
                            cmd.Parameters.AddWithValue("@Amount", dialog.SelectedAmount);
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }

                    MessageBox.Show("Loan payment successful.");
                    LoadPendingLoans(); // Refresh your UI
                    LoadApprovedLoans();
                }
            }
        }

        private void ApproveLoan_Click(object sender, RoutedEventArgs e)
        {
            UpdateLoanStatus("Approved");
        }

        private void RejectLoan_Click(object sender, RoutedEventArgs e)
        {
            UpdateLoanStatus("Rejected");
        }

        private void UpdateLoanStatus(string status)
        {
            if (PendingLoansListView.SelectedItem is Loan selectedLoan)
            {
                string accountNumber = selectedLoan.AccountNumber;
                string loanType = selectedLoan.LoanType;
                decimal amount = selectedLoan.Amount;

                using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
                {
                    connection.Open();

                    // 🔍 Check if account exists
                    string checkAccountQuery = "SELECT COUNT(1) FROM Accounts WHERE AccountNumber = @AccountNumber";
                    using (var checkCmd = new SQLiteCommand(checkAccountQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            MessageBox.Show("This account number does not exist in the database.", "Invalid Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    if (status == "Approved")
                    {
                        string updateBalanceQuery = @"UPDATE Accounts SET Balance = Balance + @Amount 
                                              WHERE AccountNumber = @AccountNumber";

                        using (var cmd = new SQLiteCommand(updateBalanceQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@Amount", amount);
                            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string updateLoanStatusQuery = @"
                    UPDATE Loans 
                    SET Status = @Status,
                        RemainingAmount = CASE 
                        WHEN RemainingAmount IS NULL THEN Amount 
                        ELSE RemainingAmount 
                        END
                    WHERE LoanID = @LoanID";

                    using (var updateStatusCmd = new SQLiteCommand(updateLoanStatusQuery, connection))
                    {
                        updateStatusCmd.Parameters.AddWithValue("@Status", status);
                        updateStatusCmd.Parameters.AddWithValue("@LoanID", selectedLoan.LoanID);
                        updateStatusCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Loan {status.ToLower()} successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPendingLoans();
                LoadApprovedLoans();
            }
        }
    }
}
