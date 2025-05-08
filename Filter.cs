using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Tracker
{

    // Filter is an extendable class to create custom views of the data
    // base class supports dates and always returns data sorted by date
    // must use AddData to get source data

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
            this.fromDate = fromDate;
        }
        public void ClearData() { this.transactions = null; }

        //overrideable function to define how output is retrieved
        // default is to sort by date within time frame
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
        // overloaded function to sort by date and time for extended filters
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

        // get contents of data that have the same category
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

        // gets contents of data where searchterm appears in the description
        public override IEnumerable<Transaction> GetFilterOutput()
        {
            var filteredTransactions = transactions.Where(t => t.payee.Contains(searchTerm) || t.description.Contains(searchTerm));
            return base.GetFilterOutput(filteredTransactions);
        }
    }
}
