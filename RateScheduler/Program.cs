using Quartz;
using Quartz.Impl;
using System.Configuration;
using System;
using System.Threading.Tasks;
using NLog;
using ExchangeRatesPlatform.Services;

namespace RateScheduler
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            string cron = @"* 0/1 * * * ? *";
            try
            {
                cron = ConfigurationManager.AppSettings["cron"];
                RunRatesUpdater(cron).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed start updater." + ex.Message + ex.StackTrace);
            }
        }


        private static async Task RunRatesUpdater(string cron)
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();

                IJobDetail job = JobBuilder.Create<AddCurrentRateJob>()
                    .WithIdentity("RatesJob")
                    .Build();


                Console.WriteLine("Starting scheduling...");

                var trigger = TriggerBuilder.Create()
                    .ForJob(job)
                    .WithCronSchedule(cron)
                    .WithIdentity("Trigger")
                    .StartNow()
                    .Build();


                await scheduler.ScheduleJob(job, trigger);
                await scheduler.Start();

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey();

                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

    }

    public class AddCurrentRateJob : IJob
    {

        public async Task Execute(IJobExecutionContext context)
        {
            ICurrencyDataProvider currencyDataProvider = new DefaultCurrencyDataProvider();
            ICurrencyService currencyService = new DefaultCurrencyService();

            var rates = await currencyDataProvider.ParseRatesAsync(DateTime.Now);

            if(rates == null)
            {
                Console.WriteLine("Failed to parse rates.");
            }
            if (!currencyService.AddRates(rates))
            {
                Console.WriteLine("Failed to add today's currencies rates!");
                return;
            }
            Console.WriteLine("Done!");
        }
    }
}

