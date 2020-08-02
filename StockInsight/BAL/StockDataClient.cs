using System;
using System.Collections.Generic;
using StockInsight.Model;
using StockInsight.DAL;

namespace StockInsight.BAL
{
    public class StockDataClient
    {
        private static Reporter logger = new Reporter();

        /// <summary>
        /// Get stock quote data using stock data service
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Quote GetStockQuoteData(IStockDataService stockDataService, string symbol, out Error error)
        {
            Quote quote;
            error = Error.NONE;

            try
            {
                quote = stockDataService.GetStockQuoteData(symbol);
            }
            catch (Exception ex)
            {
                quote = null;
                error = Error.API;
                logger.error(ex.Message);
            }

            return quote;
        }

        /// <summary>
        /// Get stock company data using stock data service
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Company GetStockCompanyData(IStockDataService stockDataService, string symbol, out Error error)
        {
            Company company;
            error = Error.NONE;

            try
            {
                company = stockDataService.GetStockCompanyData(symbol);
            }
            catch (Exception ex)
            {
                company = null;
                error = Error.API;
                logger.error(ex.Message);
            }

            return company;
        }

        /// <summary>
        /// Get stock day chart data using stock data service
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<DayChart> GetStockDailyData(IStockDataService stockDataService, string symbol, out Error error)
        {
            List<DayChart> dayCharts;
            error = Error.NONE;

            try
            {
                dayCharts = stockDataService.GetStockDailyData(symbol);
            }
            catch (Exception ex)
            {
                dayCharts = null;
                error = Error.API;
                logger.error(ex.Message);
            }

            return dayCharts;
        }

        /// <summary>
        /// Get stock month chart data using stock data service
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<Chart> GetStockMonthlyData(IStockDataService stockDataService, string symbol, out Error error)
        {
            List<Chart> monthCharts;
            error = Error.NONE;

            try
            {
                monthCharts = stockDataService.GetStockMonthlyData(symbol);
            }
            catch (Exception ex)
            {
                monthCharts = null;
                error = Error.API;
                logger.error(ex.Message);
            }

            return monthCharts;
        }

    }
}
