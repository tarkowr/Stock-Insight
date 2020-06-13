using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
