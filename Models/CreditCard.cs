using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankManagementSystem.Models
{
    public class CreditCard : Card
    {
        public decimal CreditLimit { get; set; }

        public override void ShowCardDetails()
        {
            base.ShowCardDetails();
            Console.WriteLine($"Credit Limit: {CreditLimit}");
        }
    }
}