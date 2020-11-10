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
        private ILocalStorageService localStorageService;

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
            stockDataService = new IEXStockDataService();
            localStorageService = new XmlLocalStorageService();
        }
        #endregion

        #region Database Service
        /// <summary>
        /// Read in Watchlist from database
        /// </summary>
        /// <param name="message"></param>
        public void ReadSavedWatchlist(out Error error)
        {
            if (context.User == null)
            {
                context.Watchlist = new List<TickerSymbol>();
                error = Error.OTHER;
                return;
            }

            context.Watchlist = DatabaseClient.ReadWatchlist(databaseService, context.User?.UserId, out error);
            context.Watchlist.OrderBy(stock => stock.Symbol).ToList();
        }

        /// <summary>
        /// Add symbol to database
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="error"></param>
        public void InsertSymbol(TickerSymbol symbol, out Error error)
        {
            DatabaseClient.InsertSymbol(databaseService, symbol, out error);
        }

        /// <summary>
        /// Remove symbol from database
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="error"></param>
        public void DeleteSymbol(TickerSymbol symbol, out Error error)
        {
            DatabaseClient.DeleteSymbol(databaseService, symbol, out error);
        }
        #endregion

        #region Local Storage Service
        /// <summary>
        /// Get user from local storage or create new user if none
        /// </summary>
        /// <param name="error"></param>
        public void GetOrCreateUser(out Error error)
        {
            User user = LocalStorageClient.GetUser(localStorageService, out error);

            if (user.UserId == null)
            {
                user = new User();
                user.Create();
                LocalStorageClient.SaveUser(localStorageService, user, out error);
            }

            context.User = user;
        }

        #endregion

        #region Stock Data Service
        /// <summary>
        /// Fetch quote data for all stocks
        /// </summary>
        /// <param name="message"></param>
        public void GetAllQuoteData(out Error error)
        {
            error = Error.NONE;

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetStockQuoteData(t.Symbol, out error);
            }
        }

        /// <summary>
        /// Fetch quote data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockQuoteData(string symbol, out Error error)
        {
            var quote = StockDataClient.GetStockQuoteData(stockDataService, symbol, out error);

            if (quote == null) return;

            var stock = GetStockBySymbol(symbol, context.Stocks);

            if (stock == null)
            {
                stock = new Stock();
                context.Stocks.Add(stock);
            }

            BindQuoteToStock(stock, quote);
            BindPriceToStockProperty(stock);
            BindStockNameSymbol(stock);

            stock.Symbol = stock.Symbol ?? symbol;
        }

        /// <summary>
        /// Get stock detail data if needed
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="error"></param>
        public void GetStockDetailData(Stock stock, out Error error)  
        {
            var isTrading = AreStocksCurrentlyTrading();
            error = Error.NONE;

            var companyErr = Error.NONE;
            var dailyErr = Error.NONE;
            var monthlyErr = Error.NONE;

            if (!IsStockCompanyDataCached(stock))
            {
                GetStockCompanyData(stock.Symbol, out error);
                companyErr = error;
            }

            if (isTrading || !IsStockDailyDataCached(stock))
            {
                GetStockDailyData(stock.Symbol, out error);
                dailyErr = error;
            }

            if (isTrading || !IsStockMonthlyDataCached(stock))
            {
                GetStockMonthlyData(stock.Symbol, out error);
                monthlyErr = error;
            }

            if (!companyErr.Equals(Error.NONE) || !dailyErr.Equals(Error.NONE) || !monthlyErr.Equals(Error.NONE))
            {
                error = Error.API;
            }
        }

        /// <summary>
        /// Fetch company data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockCompanyData(string symbol, out Error error)
        {
            var company = StockDataClient.GetStockCompanyData(stockDataService, symbol, out error);

            if (company == null) return;

            var stock = GetStockBySymbol(symbol, context.Stocks);

            BindCompanyToStock(stock, company);
        }

        /// <summary>
        /// Fetch day chart data by stock
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetStockDailyData(string symbol, out Error error)
        {
            var dayCharts = StockDataClient.GetStockDailyData(stockDataService, symbol, out error);

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
        public void GetStockMonthlyData(string symbol, out Error error)
        {
            var monthCharts = StockDataClient.GetStockMonthlyData(stockDataService, symbol, out error);

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
            if (stock == null || quote == null) return;
            stock.QuoteData = quote;
        }

        /// <summary>
        /// Attach company data to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="company"></param>
        public void BindCompanyToStock(Stock stock, Company company)
        {
            if (stock == null || company == null) return;
            stock.CompanyData = company;
        }

        /// <summary>
        /// Attach day charts to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="charts"></param>
        public void BindDailyChartsToStock(Stock stock, List<DayChart> charts)
        {
            if (stock == null || charts == null) return;
            stock.DayCharts = charts;
        }

        /// <summary>
        /// Attach month charts to stock
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="charts"></param>
        public void BindMonthlyChartsToStock(Stock stock, List<Chart> charts)
        {
            if (stock == null || charts == null) return;
            stock.MonthCharts = charts;
        }

        /// <summary>
        /// Bind close and formattedClose Stock Properties
        /// Only needs to be called after fetching quote stock data
        /// </summary>
        /// <param name="stock"></param>
        private void BindPriceToStockProperty(Stock stock)
        {
            double close = (stock.QuoteData?.latestPrice ??
                stock.MonthCharts?.LastOrDefault(st => st.close != null)?.close ?? "0.0").ConvertStringToDouble();

            stock.Close = close;
            stock.FormattedClose = close.FormatStockPrice();
        }

        /// <summary>
        /// Add stock symbol and company name to stock properties
        /// Only needs to be called after fetching quote stock data
        /// </summary>
        /// <param name="stock"></param>
        private void BindStockNameSymbol(Stock stock)
        {
            string symbol = stock.QuoteData?.symbol;
            string name = stock.QuoteData?.companyName;

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

            if (!CheckWatchlistCount(context.Watchlist))
            {
                message = $"Watchlist is full.";
                return;
            }

            if (DoesStockExist(symbol, context.Stocks))
            {
                message = $"{symbol} is already in your Watchlist.";
                return;
            }

            GetStockQuoteData(symbol, out Error error);

            if (!DoesStockExist(symbol, context.Stocks) || !error.Equals(Error.NONE))
            {
                message = $"Unable to find stock symbol {symbol}.";
                return;
            }

            var tickerSymbol = CreateNewTickerSymbol(context.Watchlist, symbol);
            context.Stocks = context.Stocks.OrderBy(stock => stock.Symbol).ToList();

            InsertSymbol(tickerSymbol, out error);
        }

        /// <summary>
        /// Logic for removing a stock from the watchlist
        /// </summary>
        /// <param name="symbol"></param>
        public void RemoveStockFromWatchlist(string symbol)
        {
            if (DoesStockExist(symbol, context.Stocks))
            {
                var stockToRemove = context.Stocks.FirstOrDefault(stock => stock.Symbol == symbol);
                var symbolToRemove = context.Watchlist.FirstOrDefault(sym => sym.Symbol == symbol);

                if (stockToRemove != null && symbolToRemove != null)
                {
                    context.Stocks.Remove(stockToRemove);
                    context.Watchlist.Remove(symbolToRemove);
                    DeleteSymbol(symbolToRemove, out Error error);
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
        public bool DoesStockExist(string symbol, List<Stock> stocks)
        {
            foreach (var stock in stocks)
            {
                if (stock.Symbol.ToUpper() == symbol.ToUpper())
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
            string defaultText = "SEARCH...";
            message = "";

            if (input == "" || input == null || input == defaultText)
            {
                message = "Empty Symbol.";
                return false;
            }

            if (!Regex.IsMatch(input, @"^[a-zA-Z.]+$"))
            {
                message = "Invalid Symbol.";
                return false;
            }

            if (input.Length > maxSymbolLength)
            {
                message = "Symbol is too long.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure that the Watchlist does not have too many stocks
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        private bool CheckWatchlistCount(List<TickerSymbol> symbols)
        {
            int max = 20;

            return symbols.Count() < max;
        }

        /// <summary>
        /// Check if a string is null or empty
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool IsEmpty(string msg)
        {
            return string.IsNullOrEmpty(msg);
        }

        /// <summary>
        /// Return true/false if Watchlist is empty
        /// </summary>
        /// <returns></returns>
        public bool IsWatchlistEmpty()
        {
            return context.Watchlist.Count == 0;
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
        /// Determine whether stocks are currently trading by day of week and time (EST)
        /// </summary>
        /// <returns></returns>
        public bool AreStocksCurrentlyTrading()
        {
            var start = new TimeSpan(9, 30, 0);
            var end = new TimeSpan(16, 0, 0);

            // Get EST time of day and week day
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var now = easternTime.TimeOfDay;
            var dayOfWeek = easternTime.DayOfWeek;

            if (now < start || now > end)
            {
                return false;
            }

            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determine whether stock has company data
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool IsStockCompanyDataCached(Stock stock)
        {
            return !IsEmpty(stock.CompanyData?.symbol);
        }

        /// <summary>
        /// Determine whether stock has daily charts data
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool IsStockDailyDataCached(Stock stock)
        {
            return stock.DayCharts?.Count > 0;
        }

        /// <summary>
        /// Determine whether stock has monthly charts data
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool IsStockMonthlyDataCached(Stock stock)
        {
            return stock.MonthCharts?.Count > 0;
        }

        /// <summary>
        /// Create a new Ticker Symbol for an added stock to the watchlist
        /// </summary>
        /// <param name="tickerSymbols"></param>
        /// <param name="symbol"></param>
        private TickerSymbol CreateNewTickerSymbol(List<TickerSymbol> tickerSymbols, string symbol)
        {
            var tickerSymbol = new TickerSymbol(context.User?.UserId, symbol);
            tickerSymbols.Add(tickerSymbol);
            return tickerSymbol;
        }

        /// <summary>
        /// Return a stock by its symbol property
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        public Stock GetStockBySymbol(string symbol, List<Stock> stocks)
        {
            return stocks.Where(stock => stock.Symbol.ToUpper() == symbol.ToUpper()).FirstOrDefault();
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

            return stocks.Where(stock => stock.Symbol.Contains(input) || GetStockNameSubstring(stock?.CompanyName, length) == input).ToList();
        }

        /// <summary>
        /// Retrieve stock name substring for filtering
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private string GetStockNameSubstring(string companyName, int length)
        {
            if (companyName == null) return null;

            return new string(companyName.ToUpper().Take(length).ToArray());
        }

        /// <summary>
        /// Get the first price from a list of prices
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        public double GetFirstPrice(List<string> prices)
        {
            return (prices.FirstOrDefault(x => x != null) ?? "0").ConvertStringToDouble();
        }

        /// <summary>
        /// Get the last price from a list of prices
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        public double GetLastPrice(List<string> prices)
        {
            return (prices.LastOrDefault(x => x != null) ?? "0").ConvertStringToDouble();
        }

        /// <summary>
        /// Get Brush color based on open/close values
        /// </summary>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public Brush GetLineColor(double open, double close)
        {
            return open > close ? WindowStyles.RedSI : WindowStyles.GreenSI;
        }
        #endregion
    }
}
