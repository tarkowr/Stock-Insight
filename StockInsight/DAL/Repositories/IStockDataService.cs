using System.Collections.Generic;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public interface IStockDataService
    {
        Quote GetStockQuoteData(string symbol);
        Company GetStockCompanyData(string symbol);
        List<DayChart> GetStockDailyData(string symbol);
        List<Chart> GetStockMonthlyData(string symbol);
    }
}
