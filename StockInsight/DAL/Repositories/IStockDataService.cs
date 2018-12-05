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
        Company GetStockCompanyData(string symbol);
        List<DayChart> GetStockDailyData(string symbol);
        List<MonthChart> GetStockMonthlyData(string symbol);
        Quote GetStockQuoteData(string symbol);
    }
}
