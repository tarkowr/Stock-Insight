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
        /// Parse and return a RootObject object with Company, Quote, and Monthly data
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public RootObject GetMainStockData(string symbol)
        {
            string key = DataSettings.QuoteCompanyMonthlyData(symbol);

            RootObject rootObject = new RootObject();

            string result = HttpGetData(key);

            rootObject = JsonConvert.DeserializeObject<RootObject>(result);

            return rootObject;
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
