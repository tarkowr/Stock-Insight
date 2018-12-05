using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public class StockDataService : IStockDataService
    {

        /// <summary>
        /// Parse and return a Company object
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Company GetStockCompanyData(string symbol)
        {
            string key = DataSettings.CompanyApi(symbol);

            Company company = new Company();

            string result = HttpGetData(key);

            company = JsonConvert.DeserializeObject<Company>(result);

            return company;
        }

        /// <summary>
        /// Parse and return a List of Day Charts
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public List<DayChart> GetStockDailyData(string symbol)
        {
            string key = DataSettings.DailyDataApi(symbol);

            List<DayChart> dayChart = new List<DayChart>();

            string result = HttpGetData(key);

            dayChart = JsonConvert.DeserializeObject<List<DayChart>>(result);

            return dayChart;
        }

        /// <summary>
        /// Parse and return a List of MonthCharts
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public List<MonthChart> GetStockMonthlyData(string symbol)
        {
            string key = DataSettings.MonthlyDataApi(symbol);

            List<MonthChart> monthChart = new List<MonthChart>();

            string result = HttpGetData(key);

            monthChart = JsonConvert.DeserializeObject<List<MonthChart>>(result);

            return monthChart;
        }

        /// <summary>
        /// Parse and return a Quote object
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Quote GetStockQuoteData(string symbol)
        {
            string key = DataSettings.QuoteApi(symbol);

            Quote quote = new Quote();

            string result = HttpGetData(key);

            quote = JsonConvert.DeserializeObject<Quote>(result);

            return quote;
        }

        /// <summary>
        /// Use WebClient to make an API call
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static string HttpGetData(string url)
        {
            string result = null;

            using (WebClient syncClient = new WebClient())
            {
                result = syncClient.DownloadString(url);
            }

            return result;
        }
    }
}
