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
            new Bank("Sparebank1 BV", "SBV"),
            new Bank("Sparebank1 Gudbrandsdal", "SGD"),
            new Bank("Sparebank1 Hallingdal Valdres", "HALL"),
            new Bank("Sparebank1 Hedmark", "SHED"),
            new Bank("Sparebank1 Lom og Skjåk", "SSL"),
            new Bank("Sparebank1 Modum", "MSS"),
            new Bank("Sparebank1 Nord-Norge", "SNN"),
            new Bank("Sparebank1 Nordvest", "SNV"),
            new Bank("Sparebank1 Nøtterøy-Tønsberg", "NSS"),
            new Bank("Sparebank1 Oslo Akershus", "OSL"),
            new Bank("Sparebank1 Ringerike Hadeland", "SSR"),
            new Bank("Sparebank1 SMN", "SMN"),
            new Bank("Sparebank1 SR-Bank", "SRB"),
            new Bank("Sparebank1 Søre Sunnmøte", "SVO"),
            new Bank("Sparebank1 Telemark", "GSO"),
            new Bank("Sparebank1 Østfold Akershus", "RVM")
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
