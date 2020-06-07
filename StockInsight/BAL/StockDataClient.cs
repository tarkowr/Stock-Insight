using System;
using System.Collections.Generic;
using StockInsight.Model;
using StockInsight.DAL;


namespace StockInsight.BAL
{
    public class StockDataClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Quote GetStockQuoteData(IStockDataService stockDataService, string symbol, out string message)
        {
            Quote quote;
            message = "";

            try
            {
                quote = stockDataService.GetStockQuoteData(symbol);
            }
            catch (Exception ex)
            {
                quote = null;
                message = ex.Message;
            }

            return quote;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Company GetStockCompanyData(IStockDataService stockDataService, string symbol, out string message)
        {
            Company company;
            message = "";

            try
            {
                company = stockDataService.GetStockCompanyData(symbol);
            }
            catch (Exception ex)
            {
                company = null;
                message = ex.Message;
            }

            return company;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<DayChart> GetStockDailyData(IStockDataService stockDataService, string symbol, out string message)
        {
            List<DayChart> dayCharts;
            message = "";

            try
            {
                dayCharts = stockDataService.GetStockDailyData(symbol);
            }
            catch (Exception ex)
            {
                dayCharts = null;
                message = ex.Message;
            }

            return dayCharts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockDataService"></param>
        /// <param name="symbol"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<Chart> GetStockMonthlyData(IStockDataService stockDataService, string symbol, out string message)
        {
            List<Chart> monthCharts;
            message = "";

            try
            {
                monthCharts = stockDataService.GetStockMonthlyData(symbol);
            }
            catch (Exception ex)
            {
                monthCharts = null;
                message = ex.Message;
            }

            return monthCharts;
        }

    }
}
