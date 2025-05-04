using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace BankManagementSystem
{
    public partial class UserManagementWindow : Window
    {
        public UserManagementWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            UserListView.Items.Clear();

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();
                string query = "SELECT UserId, Username, Role FROM Users";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserListView.Items.Add(new
                        {
                            Id = reader["UserId"],
                            Username = reader["Username"],
                            Role = reader["Role"]
                        });
                    }
                }
            }
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRole = ((ComboBoxItem)RoleComboBox.SelectedItem)?.Content.ToString();

            if (selectedRole == "Admin")
            {
                AccountNumberTextBox.IsEnabled = false;
                AccountNumberTextBox.Clear();
            }
            else
            {
                AccountNumberTextBox.IsEnabled = true;
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string role = ((ComboBoxItem)RoleComboBox.SelectedItem)?.Content.ToString();
            string accountNumber = AccountNumberTextBox.Text.Trim();
            bool canLogin = CanLoginCheckBox.IsChecked == true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                MessageBox.Show("All fields are required (except Account Number for Admins).", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                // If role is Klient, verify that account number exists
                if (role == "Klient")
                {
                    if (string.IsNullOrEmpty(accountNumber))
                    {
                        MessageBox.Show("Account Number is required for Klient role.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string checkAccountQuery = "SELECT COUNT(*) FROM Accounts WHERE AccountNumber = @AccountNumber";
                    using (var checkCmd = new SQLiteCommand(checkAccountQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        long exists = (long)checkCmd.ExecuteScalar();
                        if (exists == 0)
                        {
                            MessageBox.Show("Account number not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                // Insert user
                string insertQuery = @"INSERT INTO Users (Username, PasswordHash, Role, AccountNumber, CanLogin)
                               VALUES (@Username, @PasswordHash, @Role, @AccountNumber, @CanLogin)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", password); // You may later add hashing
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@AccountNumber", role == "Admin" ? DBNull.Value : (object)accountNumber);
                    command.Parameters.AddWithValue("@CanLogin", canLogin ? 1 : 0);

                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUsers();
            ClearFields();
        }

        private void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem == null) return;

            dynamic selectedUser = UserListView.SelectedItem;
            int userId = Convert.ToInt32(selectedUser.Id);
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string role = ((ComboBoxItem)RoleComboBox.SelectedItem)?.Content.ToString();
            string accountNumber = AccountNumberTextBox.Text.Trim();
            bool canLogin = CanLoginCheckBox.IsChecked == true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(accountNumber))
            {
                MessageBox.Show("All fields are required.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();
                string query = @"UPDATE Users 
                 SET Username = @Username, 
                     PasswordHash = @Password, 
                     Role = @Role, 
                     AccountNumber = @AccountNumber, 
                     CanLogin = @CanLogin 
                 WHERE UserId = @UserId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@CanLogin", canLogin ? 1 : 0);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUsers();
            ClearFields();
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem == null) return;

            dynamic selectedUser = UserListView.SelectedItem;
            int userId = Convert.ToInt32(selectedUser.Id);

            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();
                string query = "DELETE FROM Users WHERE UserId = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("User deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUsers();
            ClearFields();
        }

        private void ClearFields()
        {
            UsernameTextBox.Clear();
            PasswordBox.Clear();
            AccountNumberTextBox.Clear();
            RoleComboBox.SelectedIndex = -1;
            CanLoginCheckBox.IsChecked = false;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserListView.SelectedItem == null) return;

            dynamic selectedUser = UserListView.SelectedItem;
            UsernameTextBox.Text = selectedUser.Username;
            // Other fields like role, etc. could also be prefilled here if stored in the object.
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
