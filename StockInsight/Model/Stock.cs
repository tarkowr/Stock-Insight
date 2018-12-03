using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.ExtensionMethods;

namespace StockInsight.Model
{
    public class Stock
    {
        public MetaData MetaData { get; set; }
        public List<TimeSeries> TimeSeries { get; set; }

        public Stock()
        {
            MetaData = new MetaData();
            TimeSeries = new List<TimeSeries>();
        }
    }

    public class MetaData
    {
        public string Name { get; set; }    
        public string Information { get; set; }
        public DateTime LastRefreshed { get; set; } 
        public string OutputSize { get; set; }
        public string Symbol { get; set; }
        public string TimeZone { get; set; }

        public MetaData()
        {

        }

        public MetaData(string _information, string _symbol, string _lastRefreshed, string _outputSize, string _timeZone)
        {
            Information = _information;
            Symbol = _symbol;
            LastRefreshed = DateTime.Parse(_lastRefreshed);
            OutputSize = _outputSize;
            TimeZone = _timeZone;
        }
    }

    public class TimeSeries
    {
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public DateTime dateTime { get; set; }

        public TimeSeries(string _open, string _high, string _low, string _close, string _volume, string _dateTime)
        {
            open = _open.ConvertStringToDouble();
            high = _high.ConvertStringToDouble();
            low = _low.ConvertStringToDouble();
            close = _close.ConvertStringToDouble();
            volume = _volume.ConvertStringToDouble();
            dateTime = DateTime.Parse(_dateTime);
        }
    }
}
