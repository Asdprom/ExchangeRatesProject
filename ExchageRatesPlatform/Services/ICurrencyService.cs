using ExchangeRatesPlatform.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExchangeRatesPlatform.Services
{
    public interface ICurrencyService
    {
        public bool AddCurrency(Currency currency);
        public bool AddRate(Rate rate);
        public bool AddRates(List<Rate> rates);
        public List<Currency> GetCurrencies();
        public List<Rate> GetRates(int year, int month, string currencyCode);
    }
}
