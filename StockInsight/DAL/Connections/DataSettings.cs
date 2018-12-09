using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.DAL
{
    static class DataSettings
    {
        public static string mongoDbConnectionString = "mongodb://tarkowr:myPassword02@nmc-shard-00-00-ikrnl.mongodb.net:27017,nmc-shard-00-01-ikrnl.mongodb.net:27017,nmc-shard-00-02-ikrnl.mongodb.net:27017/test?ssl=true&replicaSet=NMC-shard-0&authSource=admin&retryWrites=true";
        public static string mongoDbDatabase = "StockInsight";
        public static string mongoDbCollection = "ticker_symbols";

        public static string CompanyApi(string symbol)
        {
            return $"https://api.iextrading.com/1.0/stock/{symbol}/company";
        }
        public static string QuoteApi(string symbol)
        {
            return $"https://api.iextrading.com/1.0/stock/{symbol}/quote/1m";
        }
        public static string MonthlyDataApi(string symbol)
        {
            return $"https://api.iextrading.com/1.0/stock/{symbol}/chart/1m";
        }
        public static string DailyDataApi(string symbol)
        {
            return $"https://api.iextrading.com/1.0/stock/{symbol}/chart/1d?chartInterval=5";
        }
    }
}
