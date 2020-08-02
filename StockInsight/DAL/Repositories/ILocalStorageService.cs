using StockInsight.Model;

namespace StockInsight.DAL
{
    public interface ILocalStorageService
    {
        User GetUser();
        void SaveUser(User user);
    }
}
