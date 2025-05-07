using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Tracker
{
    class Transaction
    {
        public DateTime date;
        public string account;
        public string payee;
        public string description;
        public string category;
        public float amount;

        public Transaction(DateTime date, string account, string payee, string description, string category, float amount) 
        {
            this.date           = date;
            this.account        = account;
            this.payee          = payee;
            this.description    = description;
            this.category       = category;
            this.amount         = amount;
        }
    }
}
