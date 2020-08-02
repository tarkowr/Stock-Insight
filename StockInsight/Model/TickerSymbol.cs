using System;

namespace StockInsight.Model
{
    public class TickerSymbol
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Symbol { get; set; }     
        
        public TickerSymbol(string userId, string symbol)
        {
            Id = Guid.NewGuid().ToString();
            UserId = userId;
            Symbol = symbol;
        }
    }
}
