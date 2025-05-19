using System.Windows;

namespace BankManagementSystem
{
    public partial class MainWindow : Window
    {
        private readonly string userRole;
        private readonly int loggedInUserId;

        public MainWindow(int userId, string role)
        {
            InitializeComponent();
            userRole = role;
            loggedInUserId = userId;
            RestrictPermissions();
            LoadUserAccounts();
        }

        private void RestrictPermissions()
        {
            if (userRole == "Klient")
            {
                UserManagementButton.Visibility = Visibility.Collapsed;
                ManageCustomersButton.Visibility = Visibility.Collapsed;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DatabaseSetup.Initialize();
        }

        private void LoadUserAccounts()
        {
            var accounts = DatabaseHelper.GetUserAccounts(loggedInUserId);
        }

        private void OpenUserManagement_Click(object sender, RoutedEventArgs e)
        {
            UserManagementWindow userManagement = new UserManagementWindow();
            userManagement.Show();
        }

        private void OpenCustomerManagementWindow(object sender, RoutedEventArgs e)
        {
            CustomerManagementWindow customerWindow = new CustomerManagementWindow(loggedInUserId);
            customerWindow.Show();
        }

        private void OpenTransactionWindow(object sender, RoutedEventArgs e)
        {
            TransactionWindow transactionWindow = new TransactionWindow(loggedInUserId);
            transactionWindow.Show();
        }

        private void OpenAccountStatementWindow(object sender, RoutedEventArgs e)
        {
            AccountStatementWindow statementWindow = new AccountStatementWindow();
            statementWindow.Show();
        }

        private void OpenLoanWindow(object sender, RoutedEventArgs e)
        {
            LoanWindow loanWindow = new LoanWindow(userRole, loggedInUserId);
            loanWindow.Show();
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
