using System;
using System.Windows;

namespace BankManagementSystem
{
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}