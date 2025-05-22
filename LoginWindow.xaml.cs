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

            string role;
            bool canLogin;

            int userId = DatabaseHelper.AuthenticateUser(username, password, out role, out canLogin);

            if (userId != -1)
            {
                MessageBox.Show($"Welcome, {username}!");
                MainWindow mainWindow = new MainWindow(userId, role);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                if (!canLogin)
                {
                    MessageBox.Show("Your login access is disabled. Contact administrator.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Invalid credentials. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
