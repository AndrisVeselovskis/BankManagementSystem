using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankManagementSystem
{
    public class Card
    {
        public int CardID { get; set; }
        public string AccountNumber { get; set; }
        public bool IsActive { get; set; }
        public string Terms { get; set; }

        public virtual void ShowCardDetails()
        {
            Console.WriteLine($"Card ID: {CardID}, Account: {AccountNumber}, Active: {IsActive}");
        }
    }
}
