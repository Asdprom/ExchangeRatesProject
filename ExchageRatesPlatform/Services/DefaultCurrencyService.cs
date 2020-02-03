using ExchangeRatesPlatform.Model;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExchangeRatesPlatform.Services
{
    public class DefaultCurrencyService : ICurrencyService
    {
        #region Private fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly CurrencyContext currencyContext = new CurrencyContext();

        #endregion

        #region Public interface methods

        public bool AddCurrency(Currency currency)
        {
            int count = 0; 
            try
            {
                currencyContext.Currencies.Add(currency);
                count = currencyContext.SaveChanges();
            }
            catch(Exception ex)
            {
                logger.Error(ex.InnerException);
            }
            return count != 0;
        }

        public bool AddRate(Rate rate)
        {
            int count = 0; 
            try
            {
                rate.CurrencyCode = rate.Currency.Code;
                rate.Currency = null;
                currencyContext.Rates.Add(rate);
                count = currencyContext.SaveChanges();
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message + ex.InnerException);
            }
            return count != 0;
        }

        public bool AddRates(List<Rate> rates)
        {
            int count = 0;
            try
            {
                foreach (var rate in rates)
                {
                    rate.CurrencyCode = rate.Currency.Code;
                    rate.Currency = null;
                    currencyContext.Rates.Add(rate);
                }

                count = currencyContext.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + ex.InnerException);
            }
            return count == rates.Count;
        }

        public List<Currency> GetCurrencies()
        {
            var currencies = new List<Currency>();
            try
            {
                currencies = currencyContext.Currencies.ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
                return null;
            }
            return currencies;
        }

        public List<Rate> GetRates(int year, int month, string currencyCode)
        {
            List<Rate> rates = null;
            try
            {
                rates = currencyContext.Rates
                    .Where(r => r.Date.Year == year && r.Date.Month == month && r.CurrencyCode == currencyCode)
                    .OrderBy(x=> x.Date)
                    .ToList();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Failed to get rates.");
                return null;
            }
            return rates;
        }
        #endregion
    }
}
