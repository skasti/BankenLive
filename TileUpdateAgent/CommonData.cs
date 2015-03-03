using System;
using System.Collections.Generic;
using System.Text;

namespace BankenLive
{
    public class CommonData
    {
        public const String BaseUri = "https://m.sparebank1.no/personal/rest";

        public static List<Bank> Banks = new List<Bank>()
        {
            new Bank("Sparebank1 Gudbrandsdal", "SGD")
        };
    }

    public class Bank
    {
        public String Name { get; set; }
        public String Id { get; set; }

        public Bank(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
