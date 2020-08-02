using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public class IEXStockDataService : IStockDataService    
    {
        /// <summary>
        /// Parse and return quote data from API req
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Quote GetStockQuoteData(string symbol)
        {
            string key = DataSettings.Quote(symbol);

            string result = HttpGetData(key);

            return JsonConvert.DeserializeObject<Quote>(result);
        }

        /// <summary>
        /// Parse and return company data from API req
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Company GetStockCompanyData(string symbol)
        {
            string key = DataSettings.Company(symbol);

            string result = HttpGetData(key);

            return JsonConvert.DeserializeObject<Company>(result);
        }

        /// <summary>
        /// Parse and return list of day charts from API req
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public List<DayChart> GetStockDailyData(string symbol)
        {
            string key = DataSettings.Daily(symbol);

            string result = HttpGetData(key);

            return JsonConvert.DeserializeObject<List<DayChart>>(result);
        }

        /// <summary>
        /// Parse and return list of monthly data from API req
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public List<Chart> GetStockMonthlyData(string symbol)
        {
            string key = DataSettings.Monthly(symbol);

            string result = HttpGetData(key);

            return JsonConvert.DeserializeObject<List<Chart>>(result);
        }

        /// <summary>
        /// Use WebClient to make an API call
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static string HttpGetData(string url)
        {
            string result = null;

            using (var syncClient = new WebClient())
            {
                result = syncClient.DownloadString(url);
            }

            return result;
        }
    }
}
