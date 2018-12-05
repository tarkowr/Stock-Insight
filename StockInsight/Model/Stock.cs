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
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public double Close { get; set; }  
        public Company CompanyData { get; set; }    
        public Quote QuoteData { get; set; }
        public List<MonthChart> MonthCharts { get; set; }
        public List<DayChart> DayCharts { get; set; }

        public Stock()
        {
            CompanyData = new Company();
            QuoteData = new Quote();
            MonthCharts = new List<MonthChart>();
            DayCharts = new List<DayChart>();
        }
    }

    public class Quote
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public string primaryExchange { get; set; }
        public string sector { get; set; }
        public string calculationPrice { get; set; }
        public string open { get; set; }
        public string openTime { get; set; }
        public string close { get; set; }
        public string closeTime { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string latestPrice { get; set; }
        public string latestSource { get; set; }
        public string latestTime { get; set; }
        public string latestUpdate { get; set; }
        public string latestVolume { get; set; }
        public string iexRealtimePrice { get; set; }
        public string iexRealtimeSize { get; set; }
        public string iexLastUpdated { get; set; }
        public string delayedPrice { get; set; }
        public string delayedPriceTime { get; set; }
        public string extendedPrice { get; set; }
        public string extendedChange { get; set; }
        public string extendedChangePercent { get; set; }
        public string extendedPriceTime { get; set; }
        public string previousClose { get; set; }
        public string change { get; set; }
        public string changePercent { get; set; }
        public string iexMarketPercent { get; set; }
        public string iexVolume { get; set; }
        public string avgTotalVolume { get; set; }
        public string iexBidPrice { get; set; }
        public string iexBidSize { get; set; }
        public string iexAskPrice { get; set; }
        public string iexAskSize { get; set; }
        public string marketCap { get; set; }
        public string peRatio { get; set; }
        public string week52High { get; set; }
        public string week52Low { get; set; }
        public string ytdChange { get; set; }
    }

    public class MonthChart
    {
        public string date { get; set; }
        public string open { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string close { get; set; }
        public string volume { get; set; }
        public string unadjustedVolume { get; set; }
        public string change { get; set; }
        public string changePercent { get; set; }
        public string vwap { get; set; }
        public string label { get; set; }
        public string changeOverTime { get; set; }
    }

    public class DayChart
    {
        public string date { get; set; }
        public string minute { get; set; }
        public string label { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string average { get; set; }
        public string volume { get; set; }
        public string notional { get; set; }
        public string numberOfTrades { get; set; }
        public string marketHigh { get; set; }
        public string marketLow { get; set; }
        public string marketAverage { get; set; }
        public string marketVolume { get; set; }
        public string marketNotional { get; set; }
        public string marketNumberOfTrades { get; set; }
        public string open { get; set; }
        public string close { get; set; }
        public string marketOpen { get; set; }
        public string marketClose { get; set; }
        public string changeOverTime { get; set; }
        public string marketChangeOverTime { get; set; }
    }

    public class Company
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public string exchange { get; set; }
        public string industry { get; set; }
        public string website { get; set; }
        public string description { get; set; }
        public string CEO { get; set; }
        public string issueType { get; set; }
        public string sector { get; set; }
        public List<string> tags { get; set; }
    }
}
