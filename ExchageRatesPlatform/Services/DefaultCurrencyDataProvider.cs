using CsvHelper;
using ExchangeRatesPlatform.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExchangeRatesPlatform.Services
{
    public class DefaultCurrencyDataProvider : ICurrencyDataProvider
    {

        #region Private fields
        private readonly ICurrencyService currencyService = new DefaultCurrencyService();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string Delimeter = "|";
        private readonly string dailyUrl = @"https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/daily.txt";
        private readonly string yearlyUrl = @"https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/year.txt";
        #endregion

        #region Constructors
        public DefaultCurrencyDataProvider()
        {
        }
        #endregion

        #region Private methods

        private async Task<string> GetCSVFileAsync(string url)
        {
            string csv;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                using var streamReader = new StreamReader(response.GetResponseStream());
                csv = await streamReader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to download {url}");
                csv = null;
            }
            return csv;
        }

        private List<ColumnInfo> ParseHeader(string header, List<Currency> currencies)
        {
            var result = new List<ColumnInfo>();
            var columns = header.Split(Delimeter);
            for (var i = 1; i < columns.Length; i++)
            {
                var columnParts = columns[i].Split(" ");
                if (!Decimal.TryParse(columnParts[0], out var amount))
                    return null;
                Currency currency;
                if ((currency = currencies.FirstOrDefault(x => x.Code == columnParts[1])) == null)
                    return null;
                result.Add(new ColumnInfo {Amount = amount, Currency = currency});
            }
            return result;
        }

        private List<Rate> ParseRates(string line, List<ColumnInfo> columnsInfo)
        {
            var rates = new List<Rate>();
            var columns = line.Split(Delimeter);

            if (!DateTime.TryParseExact(columns[0],"dd'.'MM'.'yyyy",CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return null;
            
            for (var i = 1; i < columns.Length; i++)
            {
                var rate = new Rate();

                if (!Decimal.TryParse(columns[i], out var value))
                    return null;
                
                rate.Currency = columnsInfo[i - 1].Currency;
                rate.Date = date;
                // паранойя
                if (columnsInfo[i - 1].Amount == 0)
                    return null;
                // т.к. курс может отображаться не для 1 УЕ валюты, а для n УЕ, то приводим его к 1 УЕ
                rate.Value = value / columnsInfo[i - 1].Amount;
                rates.Add(rate);
            }
            return rates;
        }

        #endregion

        #region Inner classes
        private class ColumnInfo
        {
            public decimal Amount { get; set; }
            public Currency Currency { get; set; }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Получает курсы всех валют за день = <paramref name="date"/>
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<List<Rate>> ParseRatesAsync(DateTime date)
        {
            string url = dailyUrl + "?date=" + date.Date.ToString("dd.MM.yyyy");
            var csv = await GetCSVFileAsync(url);
            if (string.IsNullOrEmpty(csv))
            {
                return null;
            }
            var currencies = currencyService.GetCurrencies();
            if (currencies == null)
                return null;
            var rates = new List<Rate>();
            var textReader = new StringReader(csv);

            try
            {
                // убираем первые две строки, оставляем только значащие
                textReader.ReadLine();
                textReader.ReadLine();
                using (var csvReader = new CsvReader(textReader, new CultureInfo("en-US")))
                {
                    csvReader.Configuration.Delimiter = Delimeter;
                    while (csvReader.Read())
                    {
                        var rate = new Rate();
                        var currencyCode = csvReader.GetField<string>(3);
                        rate.Currency = currencies.FirstOrDefault(x => x.Code == currencyCode);
                        if (rate.Currency == null)
                            return null;
                        var amount = csvReader.GetField<decimal>(2);
                        rate.Value = csvReader.GetField<decimal>(4) / amount;
                        rate.Date = date;
                        rates.Add(rate);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to parse currencies. " + ex.Message); 
                return null;
            }
            finally
            {
                textReader.Dispose();
            }
            return rates;
        }

        /// <summary>
        /// Получает данные о курсе валют за весь год = <paramref name="year"/>
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<List<Rate>> ParseRatesAsync(int year)
        {
            string url = yearlyUrl + "?year=" + year;
            var csv = await GetCSVFileAsync(url);
            if(string.IsNullOrEmpty(csv))
            {
                return null;
            }
            var currencies = currencyService.GetCurrencies();
            if (currencies == null)
                return null;
            var rates = new List<Rate>();
            var textReader = new StringReader(csv);
            try 
            {
                var header = textReader.ReadLine();

                var columnsInfo = ParseHeader(header, currencies);
                string line;
                while((line = textReader.ReadLine()) != null)
                {
                    List<Rate> lineRates;
                    if ((lineRates = ParseRates(line, columnsInfo)) == null)
                    {
                        logger.Error("Failed to parse rates line!!");
                        continue;
                    }
                    rates.AddRange(lineRates);
                }
            }
            catch(Exception ex)
            {
                logger.Error("Failed to parse rates. " + ex.InnerException); 
                return null;
            }
            finally
            {
                textReader.Dispose();
            }
            return rates;
        }

        /// <summary>
        /// Получает список всех валют по данным сайта на сегодняшний день.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Currency>> ParseCurrenciesAsync()
        {
            string url = dailyUrl + "?date=" + DateTime.Now.Date.ToString("dd.MM.yyyy");
            var csv = await GetCSVFileAsync(url);
            if(string.IsNullOrEmpty(csv))
            {
                return null;
            }
            var currencies = new List<Currency>();
            var textReader = new StringReader(csv);
            try 
            {
                // убираем первые две строки, оставляем только значащие
                textReader.ReadLine();
                textReader.ReadLine();
                using (var csvReader = new CsvReader(textReader, new CultureInfo("en-US")))
                {
                    csvReader.Configuration.Delimiter = Delimeter;
                    while (csvReader.Read())
                    {
                        var currency = new Currency();
                        currency.Country = csvReader.GetField<string>(0);
                        currency.Code = csvReader.GetField<string>(3);
                        currencies.Add(currency);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("Failed to parse currencies. " + ex.Message); ;
            }
            finally
            {
                textReader.Dispose();
            }
            return currencies;
        }
        #endregion
    }

}
