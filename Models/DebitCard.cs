using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace BankManagementSystem.Models
    {
        public class DebitCard : Card
        {
            public decimal OverdraftLimit { get; set; }

            public override void ShowCardDetails()
            {
                base.ShowCardDetails();
                Console.WriteLine($"Overdraft Limit: {OverdraftLimit}");
            }
        }
    }