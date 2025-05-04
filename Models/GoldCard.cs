using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankManagementSystem.Models
{
    public class GoldCard : CreditCard
    {
        public decimal RewardsPoints { get; set; }

        public override void ShowCardDetails()
        {
            base.ShowCardDetails();
            Console.WriteLine($"Rewards Points: {RewardsPoints}");
        }
    }
}