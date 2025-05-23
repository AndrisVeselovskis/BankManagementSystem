using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Collections.Generic;

namespace BankManagementSystem
{
    public partial class CustomerManagementWindow : Window
    {
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
        private string _currentSortColumn = null; // Default sorting column  "AccountNumber"
        private int loggedInUserId;
        private bool isEditingMode = false; // Track if editing is active
        private Customer selectedCustomer;  // Store the selected customer
        private List<Customer> allCustomers = new List<Customer>();
        private string currentSearchQuery = "";
        private string selectedCardType = "All";
        private bool? selectedStatus = null; // null = All, true = Active, false = Inactive
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public CustomerManagementWindow()
        {
            InitializeComponent();
            CustomerListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(CustomerListView_ColumnClick));
            LoadCustomers();
            CustomerListView.Items.Refresh();
            RefreshCustomerList();
        }

        public CustomerManagementWindow(int userId)
        {
            InitializeComponent();
            loggedInUserId = userId;
            CustomerListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(CustomerListView_ColumnClick));
            LoadCustomers();
            CustomerListView.Items.Refresh();
            RefreshCustomerList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSearchQuery = SearchTextBox.Text.Trim();
            RefreshCustomerList();
        }

        private List<Customer> ApplyFilters(List<Customer> customers)
        {
            var filtered = customers;

            if (selectedCardType != "All")
                filtered = filtered.Where(c => c.CardType.Equals(selectedCardType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (selectedStatus.HasValue)
                filtered = filtered.Where(c => c.IsActive == selectedStatus.Value).ToList();

            return filtered;
        }

        private List<Customer> SearchCustomers(List<Customer> customers, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return customers;

            query = query.ToLower();
            return customers.Where(c =>
                (c.AccountNumber?.ToLower().Contains(query) ?? false) ||
                (c.FirstName?.ToLower().Contains(query) ?? false) ||
                (c.LastName?.ToLower().Contains(query) ?? false) ||
                c.Balance.ToString(CultureInfo.InvariantCulture).ToLower().Contains(query) ||
                (c.CardType?.ToLower().Contains(query) ?? false) ||
                c.IsActive.ToString().ToLower().Contains(query) ||
                (c.CardTerms?.ToLower().Contains(query) ?? false)
            ).ToList();
        }

        private void RefreshCustomerData()
        {
            LoadCustomers();
            CustomerListView.Items.Refresh();
        }

        private void RefreshCustomerList()
        {
            if (CustomerListView == null) return;

            var filtered = allCustomers;

            if (!string.IsNullOrEmpty(selectedCardType) && selectedCardType != "All")
                filtered = filtered.Where(c => c.CardType.Equals(selectedCardType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (selectedStatus.HasValue)
                filtered = filtered.Where(c => c.IsActive == selectedStatus.Value).ToList();

            if (!string.IsNullOrWhiteSpace(currentSearchQuery))
            {
                var search = currentSearchQuery.ToLower();
                filtered = filtered.Where(c =>
                    (c.AccountNumber?.ToLower().Contains(search) ?? false) ||
                    (c.FirstName?.ToLower().Contains(search) ?? false) ||
                    (c.LastName?.ToLower().Contains(search) ?? false) ||
                    c.Balance.ToString(CultureInfo.InvariantCulture).ToLower().Contains(search) ||
                    (c.CardType?.ToLower().Contains(search) ?? false) ||
                    c.IsActive.ToString().ToLower().Contains(search) ||
                    (c.CardTerms?.ToLower().Contains(search) ?? false)
                ).ToList();
            }

            var sorted = SortCustomers(filtered, _currentSortColumn, _currentSortDirection);

            CustomerListView.ItemsSource = null;
            CustomerListView.ItemsSource = sorted;
        }

        private void CardTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCardType = (CardTypeFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All";
            RefreshCustomerList();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selected == "All") selectedStatus = null;
            else if (selected == "Active") selectedStatus = true;
            else if (selected == "Inactive") selectedStatus = false;

            RefreshCustomerList();
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(CustomerListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription(sortBy, direction));
            dataView.Refresh();
        }

        private void CustomerListView_ColumnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader header && header.Tag is string tag)
            {
                ListSortDirection direction;

                // toggle direction
                if (_lastHeaderClicked == header)
                {
                    direction = _lastDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }

                _currentSortColumn = tag;
                _currentSortDirection = direction;
                _lastDirection = direction;

                // remove arrow from last column
                if (_lastHeaderClicked != null && _lastHeaderClicked != header)
                {
                    if (_lastHeaderClicked.Content is string prevContent)
                        _lastHeaderClicked.Content = prevContent.Replace(" ▲", "").Replace(" ▼", "");
                }

                // set arrow on new header
                string arrow = direction == ListSortDirection.Ascending ? " ▲" : " ▼";
                if (header.Content is string baseText)
                    header.Content = tag + arrow;

                _lastHeaderClicked = header;

                RefreshCustomerList(); // reload list with new sort
            }
        }

        private List<Customer> SortCustomers(List<Customer> customers, string column, ListSortDirection direction)
        {
            switch (column)
            {
                case "AccountNumber":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.AccountNumber).ToList()
                        : customers.OrderByDescending(c => c.AccountNumber).ToList();
                case "FirstName":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.FirstName).ToList()
                        : customers.OrderByDescending(c => c.FirstName).ToList();
                case "LastName":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.LastName).ToList()
                        : customers.OrderByDescending(c => c.LastName).ToList();
                case "Balance":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.Balance).ToList()
                        : customers.OrderByDescending(c => c.Balance).ToList();
                case "CardType":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.CardType).ToList()
                        : customers.OrderByDescending(c => c.CardType).ToList();
                case "Status":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.IsActive ? 1 : 0).ToList()
                        : customers.OrderByDescending(c => c.IsActive ? 1 : 0).ToList();
                case "Terms":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => DateTime.TryParseExact(c.CardTerms, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : DateTime.MaxValue).ToList()
                        : customers.OrderByDescending(c => DateTime.TryParseExact(c.CardTerms, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : DateTime.MinValue).ToList();
                case "HasDeposit":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.HasDeposit).ToList()
                        : customers.OrderByDescending(c => c.HasDeposit).ToList();
                case "TotalDeposits":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.TotalDeposits).ToList()
                        : customers.OrderByDescending(c => c.TotalDeposits).ToList();
                case "HasLoan":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.ActiveLoanAmount > 0 ? 1 : 0).ToList()
                        : customers.OrderByDescending(c => c.ActiveLoanAmount > 0 ? 1 : 0).ToList();
                case "ActiveLoanAmount":
                    return direction == ListSortDirection.Ascending
                        ? customers.OrderBy(c => c.ActiveLoanAmount).ToList()
                        : customers.OrderByDescending(c => c.ActiveLoanAmount).ToList();
                default:
                    return customers;
            }
        }

        private void LoadCustomers()
        {
            List<Customer> customers = new List<Customer>();
            List<string> expiredAccounts = new List<string>();

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                string query = @"
            SELECT 
                A.AccountNumber, A.FirstName, A.LastName, A.Balance, 
                COALESCE(C.CardType, 'No Card') AS CardType, 
                COALESCE(C.IsActive, 0) AS IsActive, 
                COALESCE(C.CardTerms, 'N/A') AS CardTerms,
                COALESCE((
                    SELECT SUM(CASE 
                        WHEN T.Type = 'Deposit' THEN T.Amount 
                        WHEN T.Type = 'Withdraw' THEN -T.Amount 
                        ELSE 0 
                        END)
                        FROM Transactions T
                        WHERE T.AccountNumber = A.AccountNumber
                    ), 0) AS TotalDeposits,
                COALESCE((
                    SELECT SUM(L.RemainingAmount)
                    FROM Loans L
                    WHERE L.AccountNumber = A.AccountNumber
                    AND L.Status = 'Approved'
                    ), 0) AS ActiveLoanAmount
                FROM Accounts A
                LEFT JOIN Cards C ON A.AccountNumber = C.AccountNumber";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string accountNumber = reader["AccountNumber"].ToString();
                        string cardType = reader["CardType"].ToString();
                        bool isActive = Convert.ToBoolean(reader["IsActive"]);
                        string cardTerms = reader["CardTerms"].ToString();
                        bool isExpired = false;

                        // Check if card is expired
                        if (DateTime.TryParseExact(cardTerms, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiryDate))
                        {
                            if (expiryDate < DateTime.Today && isActive)
                            {
                                isActive = false;
                                isExpired = true;
                                expiredAccounts.Add(accountNumber);
                            }
                        }

                        customers.Add(new Customer
                        {
                            AccountNumber = accountNumber,
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Balance = Convert.ToDecimal(reader["Balance"]),
                            CardType = cardType,
                            IsActive = isActive,
                            CardTerms = cardTerms,
                            TotalDeposits = reader["TotalDeposits"] != DBNull.Value
                                ? Convert.ToDecimal(reader["TotalDeposits"])
                                : 0,
                            ActiveLoanAmount = reader["ActiveLoanAmount"] != DBNull.Value
                                ? Convert.ToDecimal(reader["ActiveLoanAmount"])
                                : 0
                        });
                    }
                }
            }

            // Now update expired cards AFTER reading
            if (expiredAccounts.Count > 0)
            {
                using (var updateConn = new SQLiteConnection("Data Source=bank.db;Version=3;"))
                {
                    updateConn.Open();
                    foreach (var accNum in expiredAccounts)
                    {
                        using (var updateCmd = new SQLiteCommand("UPDATE Cards SET IsActive = 0 WHERE AccountNumber = @AccountNumber", updateConn))
                        {
                            updateCmd.Parameters.AddWithValue("@AccountNumber", accNum);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            if (customers.Count > 0)
            {
                customers = SortCustomers(customers, _currentSortColumn, _currentSortDirection);
            }

            allCustomers = customers;

            CustomerListView.ItemsSource = null; 
            CustomerListView.ItemsSource = allCustomers;
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = AccountNumberTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string selectedCardType = (CardTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool isCreditCard = selectedCardType == "Credit";
            bool isActive = CardActiveCheckBox.IsChecked == true;
            string cardTerms = CardTermsPicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "";

            if (!decimal.TryParse(BalanceTextBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal balance))
            {
                MessageBox.Show("Invalid balance format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (isCreditCard)
            {
                if (balance < -2000)
                {
                    MessageBox.Show("Credit card balance cannot go below -2000.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                if (balance < 0)
                {
                    MessageBox.Show("Only credit cards can have negative balances.", "Invalid Balance", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                // Check if Account Exists
                string checkQuery = "SELECT COUNT(*) FROM Accounts WHERE AccountNumber = @AccountNumber";
                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Error: Account number already exists!", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Insert Customer
                string insertAccountQuery = "INSERT INTO Accounts (AccountNumber, FirstName, LastName, Balance, UserID) VALUES (@AccountNumber, @FirstName, @LastName, @Balance, @UserID)";
                using (var command = new SQLiteCommand(insertAccountQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@Balance", Math.Round(balance, 2));
                    command.Parameters.AddWithValue("@UserID", loggedInUserId);
                    command.ExecuteNonQuery();
                }

                // Insert Card Details Only if a Card is Selected
                if (selectedCardType != "None")
                {
                    string insertCardQuery = "INSERT INTO Cards (AccountNumber, CardType, IsActive, CardTerms) VALUES (@AccountNumber, @CardType, @IsActive, @CardTerms)";
                    using (var cardCommand = new SQLiteCommand(insertCardQuery, connection))
                    {
                        cardCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        cardCommand.Parameters.AddWithValue("@CardType", selectedCardType);
                        cardCommand.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                        cardCommand.Parameters.AddWithValue("@CardTerms", string.IsNullOrEmpty(cardTerms) ? "Default Terms" : cardTerms);
                        cardCommand.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Customer added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearInputFields();
            LoadCustomers();
            CustomerListView.Items.Refresh();
        }

        private void BalanceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            if (!Regex.IsMatch(newText, @"^-?\d*([.,]\d{0,2})?$"))
            {
                e.Handled = true;
            }
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditingMode)
            {
                // Step 1: Load selected customer data
                if (CustomerListView.SelectedItem == null)
                {
                    MessageBox.Show("Please select a customer to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                selectedCustomer = (Customer)CustomerListView.SelectedItem;

                foreach (ComboBoxItem item in CardTypeComboBox.Items)
                {
                    if (item.Content.ToString() == selectedCustomer.CardType)
                    {
                        CardTypeComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Populate fields with selected customer info
                AccountNumberTextBox.Text = selectedCustomer.AccountNumber;
                FirstNameTextBox.Text = selectedCustomer.FirstName;
                LastNameTextBox.Text = selectedCustomer.LastName;
                BalanceTextBox.Text = selectedCustomer.Balance.ToString();

                // Populate card fields
                CardTypeComboBox.SelectedItem = selectedCustomer.CardType;
                CardActiveCheckBox.IsChecked = selectedCustomer.IsActive;
                if (DateTime.TryParseExact(selectedCustomer.CardTerms, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    CardTermsPicker.SelectedDate = parsedDate;
                }
                else
                {
                    CardTermsPicker.SelectedDate = null;
                }

                isEditingMode = true; // Now in editing mode
                MessageBox.Show("Edit the customer details and click the Edit button again to save.", "Editing Mode", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Step 2: Save changes to the database
                using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
                {
                    connection.Open();

                    // **Update Customer Details (Name, Surname, Balance)**
                    string updateAccountQuery = "UPDATE Accounts SET FirstName = @FirstName, LastName = @LastName, Balance = @Balance WHERE AccountNumber = @AccountNumber";
                    using (var command = new SQLiteCommand(updateAccountQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                        command.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                        command.Parameters.AddWithValue("@Balance", Convert.ToDecimal(BalanceTextBox.Text.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@AccountNumber", selectedCustomer.AccountNumber);
                        command.ExecuteNonQuery();
                    }

                    // **Update or Insert Card Details**
                    string selectedCardType = (CardTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    bool isActive = CardActiveCheckBox.IsChecked == true;
                    string cardTerms = CardTermsPicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "";

                    bool isCreditCard = selectedCardType == "Credit";

                    if (!decimal.TryParse(BalanceTextBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal balance))
                    {
                        MessageBox.Show("Invalid balance format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (isCreditCard)
                    {
                        if (balance < -2500)
                        {
                            MessageBox.Show("Credit card balance cannot go below -2500.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    else
                    {
                        if (balance < 0)
                        {
                            MessageBox.Show("Only credit cards can have negative balances.", "Invalid Balance", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // **Check if the card already exists for this account**
                    string checkCardQuery = "SELECT COUNT(*) FROM Cards WHERE AccountNumber = @AccountNumber";
                    using (var checkCardCommand = new SQLiteCommand(checkCardQuery, connection))
                    {
                        checkCardCommand.Parameters.AddWithValue("@AccountNumber", selectedCustomer.AccountNumber);
                        int cardExists = Convert.ToInt32(checkCardCommand.ExecuteScalar());

                        if (cardExists > 0)
                        {
                            // **Update existing card**
                            string updateCardQuery = "UPDATE Cards SET CardType = @CardType, IsActive = @IsActive, CardTerms = @CardTerms WHERE AccountNumber = @AccountNumber";
                            using (var cardCommand = new SQLiteCommand(updateCardQuery, connection))
                            {
                                cardCommand.Parameters.AddWithValue("@CardType", selectedCardType);
                                cardCommand.Parameters.AddWithValue("@IsActive", isActive);
                                cardCommand.Parameters.AddWithValue("@CardTerms", cardTerms);
                                cardCommand.Parameters.AddWithValue("@AccountNumber", selectedCustomer.AccountNumber);
                                int rowsAffected = cardCommand.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show("Error: Card update failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            // **Insert new card if none exists**
                            string insertCardQuery = "INSERT INTO Cards (AccountNumber, CardType, IsActive, CardTerms) VALUES (@AccountNumber, @CardType, @IsActive, @CardTerms)";
                            using (var cardCommand = new SQLiteCommand(insertCardQuery, connection))
                            {
                                cardCommand.Parameters.AddWithValue("@AccountNumber", selectedCustomer.AccountNumber);
                                cardCommand.Parameters.AddWithValue("@CardType", selectedCardType);
                                cardCommand.Parameters.AddWithValue("@IsActive", isActive);
                                cardCommand.Parameters.AddWithValue("@CardTerms", cardTerms);
                                cardCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // Refresh UI
                LoadCustomers();
                CustomerListView.Items.Refresh();
                MessageBox.Show("Customer details updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Step 4: Refresh the UI
                LoadCustomers(); // Reload the customers list from DB
                CustomerListView.Items.Refresh(); // Refresh ListView UI
                ClearInputFields();

                // Reset editing mode
                isEditingMode = false;
                selectedCustomer = null;
            }
        }

        private void ClearInputFields()
        {
            AccountNumberTextBox.Text = "";
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            BalanceTextBox.Text = "";
            CardTermsPicker.SelectedDate = null;

            CardTypeComboBox.SelectedIndex = -1;
            CardActiveCheckBox.IsChecked = false;
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerListView.SelectedItem is Customer selectedCustomer)
            {
                using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
                {
                    connection.Open();

                                string deleteCardsQuery = "DELETE FROM Cards WHERE AccountNumber=@AccountNumber";
            using (var cardCommand = new SQLiteCommand(deleteCardsQuery, connection))
            {
                cardCommand.Parameters.AddWithValue("@AccountNumber", selectedCustomer.AccountNumber);
                cardCommand.ExecuteNonQuery();
            }

                    string deleteQuery = "DELETE FROM Accounts WHERE AccountNumber = @AccountNumber";
                    using (var deleteCommand = new SQLiteCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@AccountNumber", selectedCustomer.AccountNumber);
                        deleteCommand.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCustomers();
                CustomerListView.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Customer
    {
        public string AccountNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Balance { get; set; }

        public string CardType { get; set; } = "No Card";   // Default to "No Card"
        public bool IsActive { get; set; } = false;         // Default inactive
        public string CardTerms { get; set; } = "N/A";      // Default empty terms
        public string StatusText => IsActive ? "Active" : "Inactive";

        public string HasDeposit => TotalDeposits > 0 ? "Yes" : "No";
        public decimal TotalDeposits { get; set; }

        public decimal ActiveLoanAmount { get; set; }
        public string HasLoan => ActiveLoanAmount > 0 ? "Yes" : "No";
    }
}
