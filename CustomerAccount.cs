using System;

namespace BankManagementSystem
{
    public class CustomerAccount : Account
    {
        public void SetBalance(decimal amount) => Balance = amount;

        public override void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");
            Balance += amount;
        }

        public override void Withdraw(decimal amount)
        {
            if (amount <= 0 || amount > Balance) throw new InvalidOperationException("Invalid withdrawal amount.");
            Balance -= amount;
        }
    }
}