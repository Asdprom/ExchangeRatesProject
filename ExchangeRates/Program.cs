using System;
using ExchangeRatesPlatform;
using ExchangeRatesPlatform.Model;
using ExchangeRatesPlatform.Services;

namespace ExchangeRates
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            ICurrencyDataProvider currencyDataProvider = new DefaultCurrencyDataProvider();
            ICurrencyService currencyService = new DefaultCurrencyService();

            var currencies = await currencyDataProvider.ParseCurrenciesAsync();

            foreach (var currency in currencies)
            {
                if(!currencyService.AddCurrency(currency))
                {
                    Console.WriteLine($"Failed to add currency {currency.Code}.");
                }
            }

            foreach (var year in new[] { 2017, 2018 })
            {
                var rates = await currencyDataProvider.ParseRatesAsync(year);
                if (rates == null)
                {
                    Console.WriteLine("Failed to add rates.");
                    continue;
                }
                if (!currencyService.AddRates(rates))
                {
                    Console.WriteLine("Failed to add rates.");
                }
            }

            Console.ReadKey();
        }
    }
}
