using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.Model;
using StockInsight.DAL;
using StockInsight.Utilities;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace StockInsight.BAL
{
    public class BusinessLayer
    {
        #region Fields and Constructors
        private Context context;
        private IDatabaseService databaseService;
        private StockDataService stockDataService;

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

            try
            {
                using (databaseService)
                {
                    context.Watchlist = databaseService.ReadWatchlist().ToList();
                    context.Watchlist = context.Watchlist.OrderBy(stock => stock.Symbol).ToList();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }

        /// <summary>
        /// Save watchlist to database
        /// </summary>
        /// <param name="message"></param>
        public void SaveWatchList(out string message)
        {
            message = "";

            try
            {
                using (databaseService)
                {
                    databaseService.SaveWatchlist(context.Watchlist.OrderBy(symbol => symbol.Id));
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }
        #endregion

        #region Stock Data Service
        /// <summary>
        /// Use stock data service to get daily data
        /// </summary>
        /// <param name="symbol"></param>
        public void GetStockDailyData(string symbol, out string message)
        {
            Stock stock = new Stock();
            message = "";

            try
            {
                if (DoesStockExist(symbol, context.Stocks))
                {
                    stock = GetStockBySymbol(symbol, context.Stocks);
                    stock.DayCharts = stockDataService.GetStockDailyData(symbol);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (stock.DayCharts.Count >= 2)
                {
                    stock.DayCharts.Reverse();
                }
            }
        }

        /// <summary>
        /// Get daily data for each item in watchlist
        /// </summary>
        public void GetAllStockDailyData(out string message)
        {
            message = "";

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetStockDailyData(t.Symbol, out message);
            }
        }

        /// <summary>
        /// Use stock data service to get company, quote, monthly data
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void GetMainStockData(string symbol, out string message)
        {
            Stock stock = new Stock();
            RootObject rootObject = new RootObject();
            message = "";

            try
            {
                rootObject = stockDataService.GetMainStockData(symbol);

                if (DoesStockExist(symbol, context.Stocks))
                {
                    stock = GetStockBySymbol(symbol, context.Stocks);
                    BindRootObjectToStock(stock, rootObject);
                }
                else
                {
                    BindRootObjectToStock(stock, rootObject);
                    context.Stocks.Add(stock);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (stock.MonthCharts.Count >= 2)
                {
                    stock.MonthCharts.Reverse();
                }

                BindPriceToStockProperty(stock);
                PopulateStockProperties(stock);
            }
        }

        /// <summary>
        /// Get company, quote, and monthly data foreach stock in watchlist
        /// </summary>
        /// <param name="message"></param>
        public void GetAllMainStockData(out string message)
        {
            message = "";

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetMainStockData(t.Symbol, out message);
            }
        }
        #endregion

        #region Binding Stock Properties
        /// <summary>
        /// Set Stock properties equal to RootObject properties
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="rootObject"></param>
        public void BindRootObjectToStock(Stock stock, RootObject rootObject)
        {
            stock.CompanyData = rootObject.company;
            stock.QuoteData = rootObject.quote;
            stock.MonthCharts = rootObject.chart;
        }

        /// <summary>
        /// Bind close and formattedClose Stock Properties
        /// </summary>
        /// <param name="stock"></param>
        private void BindPriceToStockProperty(Stock stock)
        {
            double close = 0;

            close = stock.QuoteData.latestPrice.ConvertStringToDouble();

            if (close == 0)
            {
                close = stock.MonthCharts.LastOrDefault(st => st.close != null).close.ConvertStringToDouble();

                stock.Close = close;
                stock.FormattedClose = close.FormatStockPrice();
            }
            else
            {
                stock.Close = close;
                stock.FormattedClose = close.FormatStockPrice();
            }
        }

        /// <summary>
        /// Add stock symbol and company name to stock properties
        /// </summary>
        /// <param name="stock"></param>
        private void PopulateStockProperties(Stock stock)
        {
            string symbol = stock.CompanyData.symbol;
            string name = stock.CompanyData.companyName;

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
                    GetMainStockData(symbol, out message);

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
            foreach (Stock stock in stocks)
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

            return stocks.Where(stock => stock.Symbol.Contains(input) || new string(stock.CompanyName.ToUpper().Take(length).ToArray()) == input).ToList();
        }

        /// <summary>
        /// Generate a new ticker symbol ID for a new stock that was added to the watchlist
        /// </summary>
        /// <param name="tickerSymbols"></param>
        /// <returns></returns>
        private int HandleSymbolId(List<TickerSymbol> tickerSymbols)
        {
            int uniqueId = 1000;
            List<int> ids = new List<int>();

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
            catch { }

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
            catch { }

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
