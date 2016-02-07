using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankenLive
{
    public class TransactionsWrapper
    {
        public List<Transaction> Transactions { get; set; }
    }

    public class Transaction: IEquatable<Transaction>
    {
        public int Id { get; set; }
        public int AmountInteger { get; set; }
        public int AmountFraction { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime InterestDate { get; set; }
        public bool Incoming { get; set; }

        public List<string> Tags { get; set; }

        public double Amount
        {
            get
            {
                return AmountInteger + (AmountInteger > 0 ? 0.01 : -0.01) * AmountFraction;
            }
        }

        public bool Equals(Transaction other)
        {
            //if (Id != other.Id) return false;
            if (AmountInteger != other.AmountInteger) return false;
            if (AmountFraction != other.AmountFraction) return false;
            if (Type != other.Type) return false;
            if (Description != other.Description) return false;
            if (Date != other.Date) return false;
            if (InterestDate != other.InterestDate) return false;
            if (Incoming != other.Incoming) return false;

            return true;
        }

        public override string ToString()
        {
            return String.Format("{0:dd.MM.yyyy}: {1} ({2},{3:00})", Date, Description, AmountInteger, AmountFraction);
        }
    }
}
