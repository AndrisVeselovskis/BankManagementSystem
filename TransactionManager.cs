using System;
using System.Data.SQLite;

namespace BankManagementSystem
{
    public class TransactionManager : ITransaction
    {
        private readonly string dbFile = "bank.db";

        public void ProcessTransaction(string accountNumber, decimal amount, string type)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("INSERT INTO Transactions (AccountNumber, Amount, Type, Date) VALUES (@AccountNumber, @Amount, @Type, @Date)", connection);

                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@Type", type);
                command.Parameters.AddWithValue("@Date", DateTime.Now);

                command.ExecuteNonQuery();
            }
        }
    }
}