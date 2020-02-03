using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeRatesPlatform.Model;

namespace ExchangeReport.Models
{
    public class RateInfo
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Maximum { get; set; }
        public decimal Median { get; set; }
        public decimal Minimum { get; set; }
        public override string ToString()
        {
            return $"{CurrencyCode} - max: {Maximum}, min: {Minimum}, median: {Median} ;" ;
        }
    }
}
