using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ExchangeRatesPlatform.Model
{
    public class Rate
    {
        public int ID { get; set; }
        [Required]
        public string CurrencyCode { get; set; }
        [ForeignKey("CurrencyCode")]
        public Currency Currency { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
