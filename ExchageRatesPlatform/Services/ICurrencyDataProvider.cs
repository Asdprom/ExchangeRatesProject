using ExchangeRatesPlatform.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRatesPlatform.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICurrencyDataProvider
    {
        public Task<List<Rate>> ParseRatesAsync(int year);
        public Task<List<Currency>> ParseCurrenciesAsync();
        public Task<List<Rate>> ParseRatesAsync(DateTime date);
    }
}
