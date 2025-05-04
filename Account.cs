using System;

namespace BankManagementSystem
{
    public abstract class Account
    {
        public string AccountNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Balance { get; protected set; }

        public string CardType { get; set; }
        public bool IsActive { get; set; }
        public string CardTerms { get; set; }

        public abstract void Deposit(decimal amount);
        public abstract void Withdraw(decimal amount);

        public override string ToString() => $"{AccountNumber}: {FirstName} {LastName} - Balance: {Balance:C}";
    }
}