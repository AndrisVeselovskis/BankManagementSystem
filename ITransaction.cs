namespace BankManagementSystem
{
    public interface ITransaction
    {
        void ProcessTransaction(string accountNumber, decimal amount, string type);
    }
}