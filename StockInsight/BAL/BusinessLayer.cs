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
        private IStockApiService stockApiService;
        private IStockLookupApiService stockLookupApiService;

        public BusinessLayer(Context _context)
        {
            context = _context;
            InstantiateFields();
        }

        private void InstantiateFields()
        {
            databaseService = new MongoDbService();
            stockApiService = new StockDataService();
            stockLookupApiService = new StockLookupService();
        }

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

        public void ReadStockIntradayApiData(out string message)
        {
            message = "";

            try
            {
                context.StockIntraday.Clear();

                if(context.Watchlist.Any())
                {
                    foreach (TickerSymbol t in context.Watchlist)
                    {
                        context.StockIntraday.Add(stockApiService.GetIntradayStockApiData(t.Symbol));
                    }

                    if (context.StockIntraday.Any())
                    {
                        foreach (Stock stock in context.StockIntraday)
                        {
                            if(stock.MetaData.Name == "" || stock.MetaData.Name == null)
                            {
                                GetSymbolNameAndAddToStock(stock, stock.MetaData.Symbol);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                context.StockIntraday = context.StockIntraday.OrderBy(s => s.MetaData.Name).ToList();
            }
        }

        public void ReadStockDailyApiData(string symbol, out string message)
        {
            message = "";

            if (!StockExistsInModel(symbol, context.StockDaily))
            {
                try
                {
                    context.StockDaily.Add(stockApiService.GetDailyStockApiData(symbol));
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
        }

        public void AddStockToWatchlist(string symbol, out string message)
        {
            message = "";
            symbol = symbol.ToUpper();

            if (!StockExistsInModel(symbol, context.StockIntraday))
            {
                try
                {
                    Stock stock = stockApiService.GetIntradayStockApiData(symbol);

                    if(stock != null)
                    {
                        GetSymbolNameAndAddToStock(stock, symbol);
                        context.StockIntraday.Add(stock);
                        CreateNewTickerSymbol(context.Watchlist, symbol);
                    }
                    else
                    {
                        message = $"{symbol} did not return any results."; 
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    context.StockIntraday = context.StockIntraday.OrderBy(s => s.MetaData.Name).ToList();
                }
            }
            else
            {
                message = $"{symbol} is already in your Watchlist.";
            }
        }

        public bool ValidSymbol(string input, out string message)
        {
            bool valid = false;
            message = "";

            if(input != "" && input != null)
            {
                if(Regex.IsMatch(input, @"^[a-zA-Z.]+$"))
                {
                    valid = true;
                }
                else
                {
                    message = "Symbol can not contain numbers or special characters.";
                }
            }
            else
            {
                message = "Symbol is empty.";
            }

            return valid;
        }

        public void RemoveStockFromWatchlist(string symbol)
        {
            if (StockExistsInModel(symbol, context.StockIntraday))
            {
                context.StockIntraday = context.StockIntraday.Where(x => x.MetaData.Symbol != symbol).ToList();
            }
        }

        private bool StockExistsInModel(string symbol, List<Stock> stocks)
        {
            foreach (Stock stock in stocks)
            {
                if (stock.MetaData.Symbol == symbol.ToUpper())
                {
                    return true;
                }
            }

            return false;
        }

        private void GetSymbolNameAndAddToStock(Stock stock, string symbol)
        {
            SymbolInformation symbolInformation = stockLookupApiService.GetStockSymbolInformation(symbol);
            stock.MetaData.Name = symbolInformation.ResultSet.Result.First().name.ToTitleCase();
        }

        private void CreateNewTickerSymbol(List<TickerSymbol> tickerSymbols, string symbol)
        {
            tickerSymbols.Add(new TickerSymbol(HandleSymbolId(context.Watchlist), symbol));
        }

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
