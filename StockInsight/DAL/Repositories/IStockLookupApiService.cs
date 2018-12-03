using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public interface IStockLookupApiService
    {
        SymbolInformation GetStockSymbolInformation(string symbol);
    }
}
