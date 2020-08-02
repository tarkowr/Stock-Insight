using System.Configuration;

namespace StockInsight.DAL
{
    class DataSettings
    {
        public static readonly string mongoDbConnectionString = ConfigurationManager.AppSettings["MongoDbConnStr"];
        public static readonly string mongoDbDatabase = ConfigurationManager.AppSettings["MongoDbName"];
        public static readonly string mongoDbCollection = ConfigurationManager.AppSettings["MongoDbCollection"];

        public static readonly string xmlFilePath = @"localStorage.xml";

        public static readonly string ApiBaseUrl = "https://cloud.iexapis.com/stable/stock/";
        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];

        public static string Daily(string symbol)
        {
            return $"{ApiBaseUrl}{symbol}/chart/1d?chartInterval=5&token={ApiToken}";
        }
        public static string Monthly(string symbol)
        {
            return $"{ApiBaseUrl}{symbol}/chart/1m?chartCloseOnly=true&token={ApiToken}";
        }
        public static string Quote(string symbol)
        {
            return $"{ApiBaseUrl}{symbol}/quote?token={ApiToken}";
        }
        public static string Company(string symbol)
        {
            return $"{ApiBaseUrl}{symbol}/company?token={ApiToken}";
        }
    }
}
