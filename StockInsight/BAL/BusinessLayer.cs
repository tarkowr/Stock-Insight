using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.Model;
using StockInsight.DAL;
using StockInsight.ExtensionMethods;
using System.Text.RegularExpressions;

namespace StockInsight.BAL
{
    public class BusinessLayer
    {
        private Context context;
        private IDatabaseService databaseService;
        private StockDataService stockDataService;

        public BusinessLayer(Context _context)
        {
            context = _context;
            InstantiateFields();
        }

        private void InstantiateFields()
        {
            databaseService = new MongoDbService();
            stockDataService = new StockDataService();
        }

        /// <summary>
        /// Good
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
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="message"></param>
        public void SaveWatchList(out string message)
        {
            message = "";

            try
            {
                using (databaseService)
                {
                    if (context.Watchlist.Any())
                    {
                        databaseService.SaveWatchlist(context.Watchlist);
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        public void GetStockCompanyData(string symbol, out string message)
        {
            Stock stock = new Stock();
            message = "";

            try
            {
                stock.CompanyData = stockDataService.GetStockCompanyData(symbol);
                AddStockNameAndSymbol(stock);
                context.Stocks.Add(stock);
            }
            catch(Exception ex)
            {
                message = ex.Message;
            }
        }

        public void GetAllStockCompanyData(out string message)
        {
            message = "";

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetStockCompanyData(t.Symbol, out message);
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="symbol"></param>
        public void GetStockQuoteData(string symbol, out string message)
        {
            Stock stock = new Stock();
            message = "";

            try
            {
                if(DoesStockExist(symbol, context.Stocks)){
                    stock = GetStockBySymbol(symbol, context.Stocks);
                    stock.QuoteData = stockDataService.GetStockQuoteData(symbol);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        public void GetAllStockQuoteData(out string message)
        {
            message = "";

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetStockQuoteData(t.Symbol, out message);
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="symbol"></param>
        public void GetStockMonthlyData(string symbol, out string message)
        {
            Stock stock = new Stock();
            message = "";

            try
            {
                if (DoesStockExist(symbol, context.Stocks))
                {
                    stock = GetStockBySymbol(symbol, context.Stocks);
                    stock.MonthCharts = stockDataService.GetStockMonthlyData(symbol);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (message == "" || message == null)
                {
                    if (stock.MonthCharts.Count >= 2)
                    {
                        stock.MonthCharts.Reverse();
                    }
                }
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        public void GetAllStockMonthlyData(out string message)
        {
            message = "";

            foreach (TickerSymbol t in context.Watchlist)
            {
                GetStockMonthlyData(t.Symbol, out message);
            }
        }

        /// <summary>
        /// Good
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
                    stock.Close = stock.DayCharts.Last().close.ConvertStringToDouble();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (message == "" || message == null)
                {
                    if (stock.DayCharts.Count >= 2)
                    {
                        stock.DayCharts.Reverse();
                    }
                }
            }
        }

        /// <summary>
        /// Good
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
        /// Good
        /// </summary>
        /// <param name="stock"></param>
        private void AddStockNameAndSymbol(Stock stock)
        {
            string symbol = stock.CompanyData.symbol;
            string name = stock.CompanyData.companyName;

            if(symbol != "" && symbol != null)
            {
                stock.Symbol = symbol.ToUpper();
            }

            if (name != "" && name != null)
            {
                stock.CompanyName = name;
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        public void AddStockToWatchlist(string symbol, out string message)
        {
            message = "";
            symbol = symbol.ToUpper();

            if (!DoesStockExist(symbol, context.Stocks))
            {
                GetStockCompanyData(symbol, out message);

                if(DoesStockExist(symbol, context.Stocks))
                {
                    GetStockDailyData(symbol, out message);
                    CreateNewTickerSymbol(context.Watchlist, symbol);

                    if (message == "" || message == null)
                    {
                        context.Stocks = context.Stocks.OrderBy(stock => stock.CompanyName).ToList();
                    }
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

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="symbol"></param>
        public void RemoveStockFromWatchlist(string symbol)
        {
            if (DoesStockExist(symbol, context.Stocks))
            {
                context.Stocks = context.Stocks.Where(stock => stock.Symbol != symbol).ToList();
            }
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="tickerSymbols"></param>
        /// <param name="symbol"></param>
        private void CreateNewTickerSymbol(List<TickerSymbol> tickerSymbols, string symbol)
        {
            tickerSymbols.Add(new TickerSymbol(HandleSymbolId(context.Watchlist), symbol));
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        private Stock GetStockBySymbol(string symbol, List<Stock> stocks)
        {
            return stocks.Where(stock => stock.Symbol.ToUpper() == symbol.ToUpper()).First();
        }

        /// <summary>
        /// Good
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
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ValidSymbol(string input, out string message)
        {
            int maxSymbolLength = 15;
            bool valid = false;
            message = "";

            if (input != "" && input != null && input != "SEARCH...")
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
                    message = "Symbol can not contain numbers or special characters.";
                }
            }
            else
            {
                message = "Symbol is blank.";
            }

            return valid;
        }

        /// <summary>
        /// Good
        /// </summary>
        /// <param name="tickerSymbols"></param>
        /// <returns></returns>
        private int HandleSymbolId(List<TickerSymbol> tickerSymbols)
        {
            int uniqueId;
            List<int> ids = new List<int>();

            if (!tickerSymbols.Any())
            {
                uniqueId = 1000;
            }
            else
            {
                uniqueId = tickerSymbols.Count() + 1000;
            }

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
    }
}
