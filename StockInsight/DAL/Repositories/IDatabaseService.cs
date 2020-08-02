using System;
using System.Collections.Generic;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public interface IDatabaseService : IDisposable
    {
        IEnumerable<TickerSymbol> ReadWatchlist(string userId);
        void InsertSymbol(TickerSymbol symbol);
        void DeleteSymbol(TickerSymbol symbol);
    }
}
