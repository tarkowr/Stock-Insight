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
        private static Reporter logger = new Reporter();

        /// <summary>
        /// Read in watchlist from database using database service
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<TickerSymbol> ReadWatchlist(IDatabaseService databaseService, out Error error)
        {
            IEnumerable<TickerSymbol> watchlist;
            error = Error.NONE;

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
                error = Error.DB;
                logger.error(ex.Message);
            }

            return watchlist.ToList();
        }

        /// <summary>
        /// Save watchlist to database using database service
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="watchlist"></param>
        /// <param name="message"></param>
        public static void SaveWatchlist(IDatabaseService databaseService, List<TickerSymbol> watchlist, out Error error)
        {
            error = Error.NONE;

            try
            {
                using (databaseService)
                {
                    databaseService.SaveWatchlist(watchlist.OrderBy(symbol => symbol.Id));
                }
            }
            catch (Exception ex)
            {
                error = Error.DB;
                logger.error(ex.Message);
            }
        }
    }
}
