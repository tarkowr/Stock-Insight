using System.Collections.Generic;

namespace StockInsight.Model
{
    public enum MouseIcons
    {
        DEFAULT,
        LOADING
    }

    public enum StockType
    {
        MONTH,
        DAY
    }

    public enum Error
    {
        NONE,
        DB,
        API,
        LOCALSTORAGE,
        OTHER
    }

    public class Context
    {
        public List<TickerSymbol> Watchlist { get; set; }
        public List<Stock> Stocks { get; set; }
        public User User { get; set; }

        public Context()
        {
            Watchlist = new List<TickerSymbol>();
            Stocks = new List<Stock>();
        }
    }
}
