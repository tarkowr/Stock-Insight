using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public interface ILocalStorageService
    {
        User GetUser();
        void SaveUser(User user);
    }
}
