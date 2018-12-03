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
    public class StockLookupService : IStockLookupApiService
    {
        public SymbolInformation GetStockSymbolInformation(string symbol)   
        {
            string key = DataSettings.DynamicApiKey(symbol);

            SymbolInformation symbolInformation = new SymbolInformation();

            SymbolInformation getCurrentStocks = HttpGetCurrentStocks(key);

            symbolInformation = getCurrentStocks;

            return symbolInformation;
        }

        /// <summary>
        /// Use WebClient to make an API call and Convert the JSON file into a WeatherData object
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static SymbolInformation HttpGetCurrentStocks(string url)
        {
            string result = null;

            using (WebClient syncClient = new WebClient())
            {
                result = syncClient.DownloadString(url);
            }

            SymbolInformation symbolInformation = JsonConvert.DeserializeObject<SymbolInformation>(result);

            return symbolInformation;
        }
    }
}
