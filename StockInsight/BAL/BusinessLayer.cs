using System;
using System.Collections.Generic;
using System.Linq;
using StockInsight.Model;
using StockInsight.DAL;
using StockInsight.Utilities;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace StockInsight.BAL
{
    public class BusinessLayer
    {
        #region Fields and Constructors
        private Context context;
        private IDatabaseService databaseService;
        private IStockDataService stockDataService;

        public BusinessLayer(Context _context)
        {
            context = _context;
            InstantiateFields();
        }

        /// <summary>
        /// Instantiate Data Services
        /// </summary>
        private void InstantiateFields()
        {
            databaseService = new MongoDbService();
            stockDataService = new StockDataService();
        }
        #endregion

        #region Database Service
        /// <summary>
        /// Read in Watchlist from database
        /// </summary>
        /// <param name="message"></param>
        public void ReadSavedWatchlist(out string message)
        {
            message = "";

            context.Watchlist = DatabaseClient.ReadWatchlist(databaseService, out message);
            context.Watchlist.OrderBy(stock => stock.Symbol).ToList();
        }

        /// <summary>
        /// Save watchlist to database
        /// </summary>
        /// <param name="message"></param>
        public void SaveWatchlist(out string message)
        {
            message = "";
            var watchlist = context.Watchlist.OrderBy(symbol => symbol.Id).ToList();

            DatabaseClient.SaveWatchlist(databaseService, watchlist, out message);
        }
        #endregion

        #region Stock Data Service
        /// <summary>
        /// Fetch quote data for all stocks
        /// </summary>
        /// <param name="message"></param>
        public void GetAllQuoteData(out string message)
        {
            message = "";

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetStockQuoteData(t.Symbol, out message);
            }
        }

        /// <summary>
        /// Fetch quote data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockQuoteData(string symbol, out string message)
        {
            var stock = new Stock();
            message = "";

            var quote = StockDataClient.GetStockQuoteData(stockDataService, symbol, out message);

            if (quote == null) return;

            if (DoesStockExist(symbol, context.Stocks))
            {
                stock = GetStockBySymbol(symbol, context.Stocks);
            }
            else
            {
                context.Stocks.Add(stock);
            }

            BindQuoteToStock(stock, quote);
            BindPriceToStockProperty(stock);
            BindStockNameSymbol(stock);
        }

        /// <summary>
        /// Fetch company data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockCompanyData(string symbol, out string message)
        {
            message = "";

            if (!DoesStockExist(symbol, context.Stocks)) return;

            var company = StockDataClient.GetStockCompanyData(stockDataService, symbol, out message);

            if (company == null) return;

            var stock = GetStockBySymbol(symbol, context.Stocks);

            BindCompanyToStock(stock, company);
        }

        /// <summary>
        /// Fetch day chart data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockDailyData(string symbol, out string message)
        {
            message = "";

            if (!DoesStockExist(symbol, context.Stocks)) return;

            var dayCharts = StockDataClient.GetStockDailyData(stockDataService, symbol, out message);

            if (dayCharts == null) return;

            dayCharts.Reverse();

            var stock = GetStockBySymbol(symbol, context.Stocks);

            BindDailyChartsToStock(stock, dayCharts);
        }

        /// <summary>
        /// Fetch month chart data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockMonthlyData(string symbol, out string message)
        {
            message = "";

            if (!DoesStockExist(symbol, context.Stocks)) return;

            var monthCharts = StockDataClient.GetStockMonthlyData(stockDataService, symbol, out message);

            if (monthCharts == null) return;

            monthCharts.Reverse();

            var stock = GetStockBySymbol(symbol, context.Stocks);

            BindMonthlyChartsToStock(stock, monthCharts);
        }
        #endregion

        #region Binding Stock Properties
        /// <summary>
        /// Attach quote data to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="quote"></param>
        public void BindQuoteToStock(Stock stock, Quote quote)
        {
            stock.QuoteData = quote;
        }

        /// <summary>
        /// Attach company data to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="company"></param>
        public void BindCompanyToStock(Stock stock, Company company)
        {
            stock.CompanyData = company;
        }

        /// <summary>
        /// Attach day charts to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="charts"></param>
        public void BindDailyChartsToStock(Stock stock, List<DayChart> charts)
        {
            stock.DayCharts = charts;
        }

        /// <summary>
        /// Attach month charts to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="charts"></param>
        public void BindMonthlyChartsToStock(Stock stock, List<Chart> charts)
        {
            stock.MonthCharts = charts;
        }

        /// <summary>
        /// Bind close and formattedClose Stock Properties
        /// </summary>
        /// <param name="stock"></param>
        private void BindPriceToStockProperty(Stock stock)
        {
            double close = (stock.QuoteData?.latestPrice ??
                stock.MonthCharts.LastOrDefault(st => st.close != null)?.close ?? "0.0").ConvertStringToDouble();

            stock.Close = close;
            stock.FormattedClose = close.FormatStockPrice();
        }

        /// <summary>
        /// Add stock symbol and company name to stock properties
        /// </summary>
        /// <param name="stock"></param>
        private void BindStockNameSymbol(Stock stock)
        {
            string symbol = stock.QuoteData.symbol;
            string name = stock.QuoteData.companyName;

            if (!IsEmpty(symbol))
            {
                stock.Symbol = symbol.ToUpper();
            }

            if (!IsEmpty(name))
            {
                stock.CompanyName = name;
            }
        }
        #endregion

        #region Watchlist Controls
        /// <summary>
        /// Logic for adding a stock to the watchlist with error handling
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void AddStockToWatchlist(string symbol, out string message)
        {
            message = "";
            symbol = symbol.ToUpper();

            if (CheckWatchlistCount(context.Watchlist))
            {
                if (!DoesStockExist(symbol, context.Stocks))
                {
                    GetStockQuoteData(symbol, out message);

                    if (DoesStockExist(symbol, context.Stocks))
                    {
                        CreateNewTickerSymbol(context.Watchlist, symbol);
                        context.Stocks = context.Stocks.OrderBy(stock => stock.Symbol).ToList();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    message = $"{symbol} is already in your Watchlist.";
                }
            }
            else
            {
                message = $"Watchlist is full.";
            }
        }

        /// <summary>
        /// Logic for removing a stock from the watchlist
        /// </summary>
        /// <param name="symbol"></param>
        public void RemoveStockFromWatchlist(string symbol)
        {
            if (DoesStockExist(symbol, context.Stocks))
            {
                var stockToRemove = context.Stocks.SingleOrDefault(stock => stock.Symbol == symbol);
                var symbolToRemove = context.Watchlist.SingleOrDefault(sym => sym.Symbol == symbol);

                if (stockToRemove != null && symbolToRemove != null)
                {
                    context.Stocks.Remove(stockToRemove);
                    context.Watchlist.Remove(symbolToRemove);
                }
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Check if a stock already exists in a list of stocks by its symbol property
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        private bool DoesStockExist(string symbol, List<Stock> stocks)
        {
            foreach (var stock in stocks)
            {
                if (stock?.Symbol.ToUpper() == symbol.ToUpper())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the user entered a valid symbol in the search textbox
        /// </summary>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ValidSymbol(string input, out string message)
        {
            int maxSymbolLength = 15;
            bool valid = false;
            string defaultText = "SEARCH...";
            message = "";

            if (input != "" && input != null && input != defaultText)
            {
                if (Regex.IsMatch(input, @"^[a-zA-Z.]+$"))
                {
                    if (input.Length <= maxSymbolLength)
                    {
                        valid = true;
                    }
                    else
                    {
                        message = "Symbol is too long.";
                    }
                }
                else
                {
                    message = "Invalid Symbol.";
                }
            }
            else
            {
                message = "Empty Symbol.";
            }

            return valid;
        }

        /// <summary>
        /// Ensure that the Watchlist does not have too many stocks
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        private bool CheckWatchlistCount(List<TickerSymbol> symbols)
        {
            int max = 30;

            if (symbols.Count() >= max)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if a string is null or empty
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool IsEmpty(string msg)
        {
            return string.IsNullOrEmpty(msg) ? true : false;
        }

        /// <summary>
        /// Return true/false if Watchlist is empty
        /// </summary>
        /// <returns></returns>
        public bool IsWatchlistEmpty()
        {
            return context.Watchlist.Count > 0 ? false : true;
        }
        #endregion

        #region Calculations
        /// <summary>
        /// Calculate the percent change between two values
        /// </summary>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public double CalculateChangePercentage(double open, double close)
        {
            return Math.Round((close - open) / open, 4) * 100;
        }

        /// <summary>
        /// Calculate gain/loss from open to close
        /// </summary>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public double CalculateGainLoss(double open, double close)
        {
            return close - open;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Create a new Ticker Symbol for an added stock to the watchlist
        /// </summary>
        /// <param name="tickerSymbols"></param>
        /// <param name="symbol"></param>
        private void CreateNewTickerSymbol(List<TickerSymbol> tickerSymbols, string symbol)
        {
            tickerSymbols.Add(new TickerSymbol(HandleSymbolId(context.Watchlist), symbol));
        }

        /// <summary>
        /// Return a stock by its symbol property
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        public Stock GetStockBySymbol(string symbol, List<Stock> stocks)
        {
            return stocks.Where(stock => stock?.Symbol.ToUpper() == symbol.ToUpper()).FirstOrDefault();
        }

        /// <summary>
        /// Return a List of Stocks that is filtered by an input
        /// </summary>
        /// <param name="stocks"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<Stock> ReturnFilteredStocks(List<Stock> stocks, string input)
        {
            input = input.ToUpper();
            int length = input.Length;

            return stocks.Where(stock => stock.Symbol.Contains(input) || new string(stock?.CompanyName.ToUpper().Take(length).ToArray()) == input).ToList();
        }

        /// <summary>
        /// Generate a new ticker symbol ID for a new stock that was added to the watchlist
        /// </summary>
        /// <param name="tickerSymbols"></param>
        /// <returns></returns>
        private int HandleSymbolId(List<TickerSymbol> tickerSymbols)
        {
            int uniqueId = 1000;
            var ids = new List<int>();

            foreach (var ts in tickerSymbols)
            {
                ids.Add(ts.Id);
            }

            while (ids.Contains(uniqueId))
            {
                uniqueId++;
            }

            return uniqueId;
        }

        /// <summary>
        /// Get the first price from a list of prices
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        public double GetFirstPrice(List<string> prices)
        {
            double price = 0;

            try
            {
                price = prices.FirstOrDefault(x => x != null).ConvertStringToDouble();
            }
            catch {
                price = 0;
            }

            return price;
        }

        /// <summary>
        /// Get the last price from a list of prices
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        public double GetLastPrice(List<string> prices)
        {
            double price = 0;
            try
            {
                price = prices.LastOrDefault(x => x != null).ConvertStringToDouble();
            }
            catch {
                price = 0;
            }

            return price;
        }

        /// <summary>
        /// Get Brush color based on open/close values
        /// </summary>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public Brush GetLineColor(double open, double close)
        {
            if (open > close)
            {
                return WindowStyles.RedSI;
            }

            return WindowStyles.GreenSI;
        }
        #endregion
    }
}
