using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public class MongoDbService : IDatabaseService
    {
        static string _connectionString;
        static string _database;
        static string _collection;
        List<TickerSymbol> tickerSymbol;

        public MongoDbService()
        {
            _connectionString = DataSettings.mongoDbConnectionString;
            _database = DataSettings.mongoDbDatabase;
            _collection = DataSettings.mongoDbCollection;
            tickerSymbol = new List<TickerSymbol>();
        }

        /// <summary>
        /// Read mongoDb for a list of TickerSymbol objects
        /// </summary>
        /// <returns>List of TickerSymbol</returns>
        public IEnumerable<TickerSymbol> ReadWatchlist(string userId)
        {
            try
            {
                var tickerSymbolList = GetCharacterColletion();

                tickerSymbol = tickerSymbolList.Find(Builders<TickerSymbol>.Filter.Where(ts => ts.UserId == userId)).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return tickerSymbol;
        }

        /// <summary>
        /// Insert new symbol into mongoDb
        /// </summary>
        /// <param name="tickerSymbol">list of tickerSymbols</param>
        public void InsertSymbol(TickerSymbol symbol)
        {
            try
            {
                var tickerSymbolList = GetCharacterColletion();

                tickerSymbolList.InsertOne(symbol);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Remove symbol from mongoDb
        /// </summary>
        /// <param name="tickerSymbol">list of tickerSymbols</param>
        public void DeleteSymbol(TickerSymbol symbol)
        {
            try
            {
                var tickerSymbolList = GetCharacterColletion();

                tickerSymbolList.DeleteOne(ts => ts.Id == symbol.Id && ts.UserId == symbol.UserId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns MongoDB collection using db connection
        /// </summary>
        /// <returns></returns>
        private IMongoCollection<TickerSymbol> GetCharacterColletion()
        {
            var client = new MongoClient(_connectionString);
            IMongoDatabase database = client.GetDatabase(_database);
            IMongoCollection<TickerSymbol> tickerSymbolCollection = database.GetCollection<TickerSymbol>(_collection);

            return tickerSymbolCollection;
        }

        public void Dispose()
        {
            tickerSymbol = null;
        }
    }
}
