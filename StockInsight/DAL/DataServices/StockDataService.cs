using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avapi;
using Avapi.AvapiTIME_SERIES_DAILY;
using Avapi.AvapiTIME_SERIES_INTRADAY;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public class StockDataService : IStockApiService
    {
        private int numberOfDays = 30;
        private int numberOfIntervals = 78;

        /// <summary>
        /// Retrieve Stock Data via Alpha Vantage Api & Avapi Package //DAILY
        /// </summary>
        /// <param name="tickerSymbol"></param>
        /// <returns></returns>
        public Stock GetDailyStockApiData(string tickerSymbol)
        {
            var connection = SetupAvapiConnection();

            Int_TIME_SERIES_DAILY time_series_daily = connection.GetQueryObject_TIME_SERIES_DAILY();
            IAvapiResponse_TIME_SERIES_DAILY time_series_dailyResponse =
                time_series_daily.Query(tickerSymbol, Const_TIME_SERIES_DAILY.TIME_SERIES_DAILY_outputsize.compact);

            var data = time_series_dailyResponse.Data;

            Stock stock = new Stock();

            stock.MetaData = new MetaData(data.MetaData.Information, data.MetaData.Symbol, data.MetaData.LastRefreshed, data.MetaData.OutputSize, data.MetaData.TimeZone);

            foreach (var timeseries in data.TimeSeries.Take(numberOfDays))
            {
                stock.TimeSeries.Add(new TimeSeries(timeseries.open, timeseries.high, timeseries.low, timeseries.close, timeseries.volume, timeseries.DateTime));
            }

            return stock;
        }

        /// <summary>
        /// Retrieve Stock Data via Alpha Vantage Api & Avapi Package //INTRADAY
        /// </summary>
        /// <param name="tickerSymbol"></param>
        /// <returns></returns>
        public Stock GetIntradayStockApiData(string tickerSymbol)
        {
            var connection = SetupAvapiConnection();

            Int_TIME_SERIES_INTRADAY time_series_intraday = connection.GetQueryObject_TIME_SERIES_INTRADAY();
            IAvapiResponse_TIME_SERIES_INTRADAY time_series_intradayResponse =
                time_series_intraday.Query(tickerSymbol, Const_TIME_SERIES_INTRADAY.TIME_SERIES_INTRADAY_interval.n_5min);

            var data = time_series_intradayResponse.Data;

            Stock stock = new Stock();

            stock.MetaData = new MetaData(data.MetaData.Information, data.MetaData.Symbol, data.MetaData.LastRefreshed, data.MetaData.OutputSize, data.MetaData.TimeZone);

            foreach (var timeseries in data.TimeSeries.Take(numberOfIntervals))
            {
                stock.TimeSeries.Add(new TimeSeries(timeseries.open, timeseries.high, timeseries.low, timeseries.close, timeseries.volume, timeseries.DateTime));
            }

            return stock;
        }

        private IAvapiConnection SetupAvapiConnection()
        {
            IAvapiConnection connection = AvapiConnection.Instance;

            connection.Connect(DataSettings.alphaVantageApiKey);

            return connection;
        }
    }
}
