using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Tracker
{
    class Filter
    {
        protected IEnumerable<Transaction> transactions;
        DateTime? toDate = null;
        DateTime? fromDate = null;
        public Filter()
        {
        }
        public void AddData(IEnumerable<Transaction> transactions) 
        {
            this.transactions = transactions;
        }
        public void ToDate(DateTime toDate) 
        {
            this.toDate = toDate;
            
        }

        public void FromDate(DateTime fromDate)
        {
            transactions = transactions.Where(t => t.date <= fromDate);
        }
        public void ClearData() { this.transactions = null; }


        public virtual IEnumerable<Transaction> GetFilterOutput() 
        {
            IEnumerable<Transaction> returnTransactions = transactions;
            if (toDate != null) {
                returnTransactions = returnTransactions.Where(t => t.date <= toDate);
            }
            if (fromDate != null)
            {
                returnTransactions = returnTransactions.Where(t => t.date >= fromDate);
            }

            return returnTransactions.OrderBy(t => t.date); 
        }
        public virtual IEnumerable<Transaction> GetFilterOutput(IEnumerable<Transaction> returnTransactions)
        {
            if (toDate != null)
            {
                returnTransactions = returnTransactions.Where(t => t.date <= toDate);
            }
            if (fromDate != null)
            {
                returnTransactions = returnTransactions.Where(t => t.date >= fromDate);
            }

            return returnTransactions.OrderBy(t => t.date);
        }

    }

    class CategoryFilter : Filter 
    {
        string category;
        public CategoryFilter(string category) : base()
        {
            this.category = category;
        }

        public override IEnumerable<Transaction> GetFilterOutput() 
        {
            var filteredTransactions = transactions.Where(t => t.category.Equals(this.category));
            return base.GetFilterOutput(filteredTransactions);
        }
    }

    class DescriptionFilter : Filter 
    {
        string searchTerm;

        public DescriptionFilter(string searchTerm) : base()
        {
            this.searchTerm = searchTerm;
        }

        public override IEnumerable<Transaction> GetFilterOutput()
        {
            var filteredTransactions = transactions.Where(t => t.description.Contains(searchTerm));
            return base.GetFilterOutput(filteredTransactions);
        }
    }
}
