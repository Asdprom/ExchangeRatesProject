using ExchangeReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeRatesPlatform;
using ExchangeRatesPlatform.Model;
using ExchangeRatesPlatform.Services;

namespace ExchangeReport.Services
{
    public class ReportService
    {
        #region Private methods
        private static IEnumerable<IEnumerable<Rate>> SplitByWeek(List<Rate> rates)
        {
            List<List<Rate>> ratesByWeek = new List<List<Rate>>();
            List<Rate> ratesWeek = new List<Rate>();
            for (int i = 1; i < rates.Count(); i++)
            {
                if (rates[i].Date.DayOfWeek < rates[i - 1].Date.DayOfWeek)
                {
                    ratesByWeek.Add(ratesWeek);
                    ratesWeek = new List<Rate>();
                    ratesWeek.Add(rates[i]);
                }
                else
                {
                    ratesWeek.Add(rates[i]);
                }
            }
            return ratesByWeek;
        }

        private static RateInfo GetRateInfo(List<Rate> ratesWeek)
        {
            var rateInfo = new RateInfo();

            rateInfo.StartDay = ratesWeek.Min(x => x.Date.Day);
            rateInfo.EndDay = ratesWeek.Max(x => x.Date.Day);
            rateInfo.CurrencyCode = ratesWeek.FirstOrDefault().CurrencyCode;

            ratesWeek.Sort((x, y) => x.Value.CompareTo(y.Value));
            rateInfo.Minimum = ratesWeek[0].Value;
            rateInfo.Maximum = ratesWeek[ratesWeek.Count() - 1].Value;

            if (ratesWeek.Count() % 2 == 0)
            {
                var half = ratesWeek.Count() / 2;
                rateInfo.Median = (ratesWeek[half].Value + ratesWeek[half - 1].Value) / 2;
            }
            else
            {
                rateInfo.Median = ratesWeek[ratesWeek.Count() / 2].Value;
            }
            return rateInfo;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Формирует строки отчета по данным из базы данных.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="currencyCodes"></param>
        /// <returns></returns>
        public static List<List<RateInfo>> GetReportInfo(int year, int month, List<string> currencyCodes)
        {
            ICurrencyService currencyService = new DefaultCurrencyService();
            var rateReports = new List<List<RateInfo>>();
            foreach (var currencyCode in currencyCodes)
            {
                var rateReport = new List<RateInfo>();
                var rates = currencyService.GetRates(year, month, currencyCode);

                if (rates == null) continue;
                var ratesByWeek = SplitByWeek(rates);
                foreach (var rateWeek in ratesByWeek)
                {
                    var rateInfo = GetRateInfo(rateWeek.ToList());
                    rateReport.Add(rateInfo);
                }
                // отсекаем случаи, если бд ничего не вернула по валюте (например - ошибка в конфиге)
                if (rateReport.Count != 0) rateReports.Add(rateReport);
            }
            return rateReports;
        }
        #endregion
    }
}
