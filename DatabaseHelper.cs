using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;


namespace BankManagementSystem
{
    public static class DatabaseHelper
    {
        private static readonly string dbFile = "bank.db";

        public static SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();
            return connection;
        }

        public static void ExecuteQuery(string query, Action<SQLiteCommand> parameterSetter)
        {
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var command = new SQLiteCommand(query, connection))
            {
                parameterSetter(command);
                command.ExecuteNonQuery();
                transaction.Commit();
            }
        }

        public static void EnsureDatabaseIntegrity()
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new SQLiteCommand("PRAGMA integrity_check;", connection))
                {
                    var result = command.ExecuteScalar()?.ToString();
                    if (result != "ok")
                    {
                        MessageBox.Show("Database integrity check failed! Consider restoring a backup.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    

    public static int AuthenticateUser(string username, string password, out string role)
        {
            string query = "SELECT UserID, Role FROM Users WHERE Username = @Username AND PasswordHash = @Password";
            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            using (var command = new SQLiteCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        role = reader.GetString(1);
                        return reader.GetInt32(0); // Return UserID
                    }
                }
            }
            role = null;
            return -1;
        }

        public static List<Account> GetUserAccounts(int userId)
        {
            List<Account> accounts = new List<Account>();
            string query = "SELECT AccountNumber, FirstName, LastName, Balance FROM Accounts WHERE UserID = @UserID";
            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            using (var command = new SQLiteCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@UserID", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var customerAccount = new CustomerAccount
                        {
                            AccountNumber = reader.GetString(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2)
                        };
                        customerAccount.SetBalance(Convert.ToDecimal(reader["Balance"]));
                        accounts.Add(customerAccount);
                    }
                }
            }
            return accounts;
        }

        public static bool IsUserAccountOwner(int userId, string accountNumber)
        {
            using (var connection = new SQLiteConnection("Data Source=bank.db;Version=3;"))
            {
                connection.Open();

                // First, get user info
                string query = "SELECT AccountNumber, Role FROM Users WHERE UserId = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string userRole = reader["Role"].ToString();
                            string userAccount = reader["AccountNumber"]?.ToString();

                            // Allow Admins access to all accounts
                            if (userRole == "Admin")
                                return true;

                            // Clients can only access their own account
                            return userAccount == accountNumber;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsUserAdmin(int userId)
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT IsAdmin FROM Users WHERE UserID = @UserID";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    var result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
        }
    }
}
