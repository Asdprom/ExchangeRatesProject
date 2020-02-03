using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExchangeReport.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using ExchangeReport.Services;

namespace ExchangeReport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptionsSnapshot<CurrencySettings> currencySettings;

        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<CurrencySettings> currencySettings)
        {
            _logger = logger;
            this.currencySettings = currencySettings;
        }

        public IActionResult Index(string year, string month, string type = "text")
        {
            Int32.TryParse(year, out var yearParameter);
            Int32.TryParse(month, out var monthParameter);
            if (yearParameter == 0 || monthParameter == 0)
            {
                return Content("Incorrect parameters passed");
            }
            var currencyCodes = currencySettings.Value.CurrencyCodes.Split(new char[] { '|' }).ToList();

            var reportRates = ReportService.GetReportInfo(yearParameter, monthParameter, currencyCodes);
            if (reportRates.Count == 0)
            {
                return Content("Data not found or database connection not configured.");
            }
            var weekCount = reportRates.FirstOrDefault().Count;

            StringBuilder report = new StringBuilder();
            string jsonString;
            if (String.Compare(type, "text", StringComparison.OrdinalIgnoreCase) == 0)
            {
                //  проходим по каждой валюте, каждый раз беря следующую неделю
                for (var i = 0; i < weekCount; i++)
                {
                    for (var j = 0; j < reportRates.Count(); j++)
                    {
                        // если это начало строки отчета - вставляем промежуток
                        if (j == 0) report.Append($"{reportRates[j][i].StartDay}...{reportRates[j][i].EndDay}: ");

                        report.Append(reportRates[j][i].ToString());
                    }
                    report.Append(Environment.NewLine);
                }
                return Content(report.ToString());
            }
            else if (String.Compare(type, "json", StringComparison.OrdinalIgnoreCase) == 0)
            {
                jsonString = JsonSerializer.Serialize(reportRates);
                return Content(jsonString);
            }
            return Content("Unsupported report type.");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
