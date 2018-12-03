using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.DAL
{
    static class DataSettings
    {
        public static string alphaVantageApiKey = "97UTHV1VRRPFVYVO";
        public static string mongoDbConnectionString = "mongodb://tarkowr:myPassword02@nmc-shard-00-00-ikrnl.mongodb.net:27017,nmc-shard-00-01-ikrnl.mongodb.net:27017,nmc-shard-00-02-ikrnl.mongodb.net:27017/test?ssl=true&replicaSet=NMC-shard-0&authSource=admin&retryWrites=true";
        public static string mongoDbDatabase = "StockInsight";
        public static string mongoDbCollection = "ticker_symbols";
        public static string DynamicApiKey(string tickerSymbol)
        {
            return "http://autoc.finance.yahoo.com/autoc?query=" + tickerSymbol + "&region=EU&lang=en-GB";
        }
    }
}
