using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ExchangeRatesPlatform.Model
{
    public class Currency
    {
        [Key]
        public string Code { get; set; }
        public string Country { get; set; }
    }
}
