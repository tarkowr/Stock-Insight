using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
