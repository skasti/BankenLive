using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BankenLive
{
    public class AccountSnapshot
    {
        public DateTime TimeStamp = DateTime.Now;
        public string Id { get; set; }
        public string Name { get; set; }
        public string FormattedNumber { get; set; }
        public int DisposableAmountInteger { get; set; }
        public int DisposableAmountFraction { get; set; }

        public double Balance
        {
            get
            {
                return DisposableAmountInteger + (DisposableAmountInteger > 0 ? 0.01 : -0.01) * DisposableAmountFraction;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1},{2}", Name, DisposableAmountInteger,
                    DisposableAmountFraction > 0 ? DisposableAmountFraction.ToString() : "-");
        }

        public static implicit operator double(AccountSnapshot snapshot)
        {
            return snapshot.Balance;
        }
    }
}
