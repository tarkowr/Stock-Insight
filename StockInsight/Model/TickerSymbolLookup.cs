using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.Model
{
    public class Result
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string exch { get; set; }
        public string type { get; set; }
        public string exchDisp { get; set; }
        public string typeDisp { get; set; }
    }

    public class ResultSet
    {
        public string Query { get; set; }
        public List<Result> Result { get; set; }
    }

    public class SymbolInformation
    {
        public ResultSet ResultSet { get; set; }
    }
}
