using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;


namespace Transaction_Tracker
{
    public class Transaction
    {
        public DateTime date;
        public string account;
        public string payee;
        public string description;
        public string category;
        public double amount;

        public Transaction(DateTime date, string account, string payee, string description, string category, double amount) 
        {
            this.date           = date;
            this.account        = account;
            this.payee          = payee;
            this.description    = description;
            this.category       = category;
            this.amount         = amount;
        }

        public string Hash()
        {
            using (MD5 md5 = MD5.Create())
            {
                string dataToHash = $"{date},{account},{payee},{description},{category},{amount}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToHash);
                byte[] hashBytes = md5.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


    }

    public class Transactions 
    {
        private List<Transaction> transactions = new List<Transaction>();

        public Transactions() { 
        
        }

        public void Serialize(string path) 
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (Transaction transaction in transactions) 
                {
                    writer.WriteLine($"{transaction.date.ToString()},{transaction.account},{transaction.payee},{transaction.description},{transaction.category},{transaction.amount.ToString("F2")}");
                }
            }

        }
        private static Transaction ParseTransaction(string line)
        {
            var parts = line.Split(',');

            DateTime date = DateTime.Parse(parts[0]);
            string account = parts[1];
            string payee = parts[2];
            string description = parts[3];
            string category = parts[4];
            double amount = double.Parse(parts[5]);

            return new Transaction(date, account, payee, description, category, amount);
        }

        public void Deserialize(string path) 
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    transactions.Add(ParseTransaction(line));
                }
            }

        }
        public void AddSingleTransaction(Transaction transaction) 
        {
            transactions.Add(transaction);
        }

        public IEnumerable<Transaction> All => transactions;

        public void Remove(Transaction toRemove) 
        {
            foreach (Transaction transaction in transactions) {
                if (transaction.Hash().Equals(toRemove.Hash())) {
                    transactions.Remove(transaction);
                    return;
                }
            }
        }
    }

}
