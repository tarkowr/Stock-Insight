using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.DAL
{
    class DataSettings
    {
        public static string mongoDbConnectionString = "mongodb://tarkowr:myPassword02@nmc-shard-00-00-ikrnl.mongodb.net:27017,nmc-shard-00-01-ikrnl.mongodb.net:27017,nmc-shard-00-02-ikrnl.mongodb.net:27017/test?ssl=true&replicaSet=NMC-shard-0&authSource=admin&retryWrites=true";
        public static string mongoDbDatabase = "StockInsight";
        public static string mongoDbCollection = "ticker_symbols";
        public static string ApiBaseUrl = "https://cloud.iexapis.com/stable/stock/";

        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];

        public static string DailyDataApi(string symbol)
        {
            return $"{ApiBaseUrl}{symbol}/chart/1d?chartInterval=5&token={ApiToken}";
        }
        public static string QuoteCompanyMonthlyData(string symbol)
        {
            return $"{ApiBaseUrl}{symbol}/batch?types=quote,company,chart&range=1m&token={ApiToken}";
        }
    }
}
