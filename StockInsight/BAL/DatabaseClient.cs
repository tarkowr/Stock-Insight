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
        public static List<TickerSymbol> ReadWatchlist(IDatabaseService databaseService, string userId, out Error error)
        {
            IEnumerable<TickerSymbol> watchlist;
            error = Error.NONE;

            try
            {
                using (databaseService)
                {
                    watchlist = databaseService.ReadWatchlist(userId).ToList();
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
        /// Insert symbol into database using database service
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="symbol"></param>
        /// <param name="error"></param>
        public static void InsertSymbol(IDatabaseService databaseService, TickerSymbol symbol, out Error error)
        {
            error = Error.NONE;

            try
            {
                using (databaseService)
                {
                    databaseService.InsertSymbol(symbol);
                }
            }
            catch (Exception ex)
            {
                error = Error.DB;
                logger.error(ex.Message);
            }
        }

        /// <summary>
        /// Remove symbol from database using database service
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="symbol"></param>
        /// <param name="error"></param>
        public static void DeleteSymbol(IDatabaseService databaseService, TickerSymbol symbol, out Error error)
        {
            error = Error.NONE;

            try
            {
                using (databaseService)
                {
                    databaseService.DeleteSymbol(symbol);
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
