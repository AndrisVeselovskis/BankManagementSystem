using System;
using System.Data.SQLite;
using System.Windows;

namespace BankManagementSystem
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            int userId = DatabaseHelper.AuthenticateUser(username, password, out string role);
            if (userId != -1)
            {
                MessageBox.Show($"Welcome, {username}!");
                MainWindow mainWindow = new MainWindow(userId, role);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool AuthenticateUser(string username, string password, out string role)
        {
            role = null;

            string dbPath = @"C:\Users\Andris PC\Desktop\Skola\Induvidual project 3\BankManagementSystem\bin\Debug\bank.db";
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Debug: Print tables to check if "Users" exists
                using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("Tables in Database:");
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0)); // Prints each table name
                    }
                }

                // Now run the actual authentication query
                string query = "SELECT Role FROM Users WHERE Username = @Username AND PasswordHash = @Password";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        role = result.ToString();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
