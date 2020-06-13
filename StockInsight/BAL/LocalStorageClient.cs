using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockInsight.Model;
using StockInsight.DAL;

namespace StockInsight.BAL
{
    public class LocalStorageClient
    {
        private static Reporter logger = new Reporter();

        /// <summary>
        /// Retrieve user from local storage
        /// </summary>
        /// <param name="localStorageService"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static User GetUser(ILocalStorageService localStorageService, out Error error)
        {
            User user;
            error = Error.NONE;

            try
            {
                user = localStorageService.GetUser();
            }
            catch (Exception ex)
            {
                user = null;
                error = Error.LOCALSTORAGE;
                logger.error(ex.Message);
            }

            return user;
        }

        /// <summary>
        /// Insert user into local storage
        /// </summary>
        /// <param name="localStorageService"></param>
        /// <param name="user"></param>
        /// <param name="error"></param>
        public static void SaveUser(ILocalStorageService localStorageService, User user, out Error error)
        {
            error = Error.NONE;

            try
            {
                localStorageService.SaveUser(user);
            }
            catch (Exception ex)
            {
                error = Error.LOCALSTORAGE;
                logger.error(ex.Message);
            }
        }
    }
}
