using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.Model
{
    public class TickerSymbol
    {
        public int Id { get; set; }
        public string Symbol { get; set; }     
        
        public TickerSymbol(int id, string symbol)
        {
            Id = id;
            Symbol = symbol;
        }
    }
}
