using System;
using System.Data.SQLite;

namespace BankManagementSystem
{
    public static class DatabaseSetup
    {
        private static readonly string dbFile = "bank.db";

        public static void Initialize()
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();

                string dropTables = @"
                DROP TABLE IF EXISTS Users;
                DROP TABLE IF EXISTS Accounts;
                DROP TABLE IF EXISTS Transactions;
                DROP TABLE IF EXISTS Loans;";

                using (var command = new SQLiteCommand(dropTables, connection))
                {
                    command.ExecuteNonQuery();
                }

                CreateTables(connection);
                InsertSampleData(connection);
            }
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Klient'))
            );";

            string createAccountsTable = @"
            CREATE TABLE IF NOT EXISTS Accounts (
                AccountID INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountNumber TEXT UNIQUE NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Balance REAL NOT NULL,
                UserID INTEGER,
                FOREIGN KEY(UserID) REFERENCES Users(UserID)
            );";

            string createTransactionsTable = @"
            CREATE TABLE IF NOT EXISTS Transactions (
                TransactionID INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountNumber TEXT NOT NULL,
                Type TEXT NOT NULL CHECK(Type IN ('Deposit', 'Withdraw', 'Transfer')),
                Amount REAL NOT NULL,
                Date DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(AccountNumber) REFERENCES Accounts(AccountNumber)
            );";

            string createLoansTable = @"
            CREATE TABLE IF NOT EXISTS Loans (
                LoanID INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountNumber TEXT NOT NULL,
                LoanType TEXT NOT NULL CHECK(LoanType IN ('Personal', 'Home', 'Car')),
                Amount REAL NOT NULL,
                Status TEXT NOT NULL CHECK(Status IN ('Pending', 'Approved', 'Rejected')),
                FOREIGN KEY(AccountNumber) REFERENCES Accounts(AccountNumber)
            );";

            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = createUsersTable;
                command.ExecuteNonQuery();

                command.CommandText = createAccountsTable;
                command.ExecuteNonQuery();

                command.CommandText = createTransactionsTable;
                command.ExecuteNonQuery();

                command.CommandText = createLoansTable;
                command.ExecuteNonQuery();
            }
        }

        private static void InsertSampleData(SQLiteConnection connection)
        {
            string insertUsers = @"
            INSERT OR IGNORE INTO Users (Username, PasswordHash, Role) VALUES
            ('admin', 'admin123', 'Admin'),
            ('klient1', 'klient123', 'Klient');";

            string insertAccounts = @"
            INSERT OR IGNORE INTO Accounts (AccountNumber, FirstName, LastName, Balance, UserID) VALUES
            ('1001', 'Janis', 'Berzins', 5000.00, 1),
            ('1002', 'Jana', 'Kalnina', 3000.00, 2);";

            string insertTransactions = @"
            INSERT OR IGNORE INTO Transactions (AccountNumber, Type, Amount) VALUES
            ('1001', 'Deposit', 1000.00),
            ('1002', 'Withdraw', 500.00);";

            string insertLoans = @"
            INSERT OR IGNORE INTO Loans (AccountNumber, LoanType, Amount, Status) VALUES
            ('1001', 'Personal', 2000.00, 'Pending');";

            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = insertUsers;
                command.ExecuteNonQuery();

                command.CommandText = insertAccounts;
                command.ExecuteNonQuery();

                command.CommandText = insertTransactions;
                command.ExecuteNonQuery();

                command.CommandText = insertLoans;
                command.ExecuteNonQuery();
            }
        }
    }
}
