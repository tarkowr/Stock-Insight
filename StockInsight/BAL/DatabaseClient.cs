using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.Model;
using StockInsight.DAL;

namespace StockInsight.BAL
{
    public class DatabaseClient
    {
        /// <summary>
        /// Read in watchlist from database using database service
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<TickerSymbol> ReadWatchlist(IDatabaseService databaseService, out string message)
        {
            IEnumerable<TickerSymbol> watchlist;
            message = "";

            try
            {
                using (databaseService)
                {
                    watchlist = databaseService.ReadWatchlist().ToList();
                }
            }
            catch (Exception ex)
            {
                watchlist = new List<TickerSymbol>();
                message = ex.Message;
            }

            return watchlist.ToList();
        }

        /// <summary>
        /// Save watchlist to database using database service
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="watchlist"></param>
        /// <param name="message"></param>
        public static void SaveWatchlist(IDatabaseService databaseService, List<TickerSymbol> watchlist, out string message)
        {
            message = "";

            try
            {
                using (databaseService)
                {
                    databaseService.SaveWatchlist(watchlist.OrderBy(symbol => symbol.Id));
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }
    }
}
