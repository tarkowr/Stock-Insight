using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.Model
{
    public enum StockType
    {
        Intraday,
        Daily
    }

    public class Context
    {
        public List<Stock> StockIntraday { get; set; }
        public List<Stock> StockDaily { get; set; }
        public List<TickerSymbol> Watchlist { get; set; }

        public Context()
        {
            StockIntraday = new List<Stock>();
            StockDaily = new List<Stock>();
            Watchlist = new List<TickerSymbol>();
        }
    }
}
