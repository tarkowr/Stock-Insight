using System.IO;
using System.Xml.Serialization;
using StockInsight.Model;

namespace StockInsight.DAL
{
    public class XmlLocalStorageService : ILocalStorageService
    {
        private string _dataFilePath;

        public XmlLocalStorageService()
        {
            _dataFilePath = DataSettings.xmlFilePath;
        }

        /// <summary>
        /// Get user from XML file
        /// </summary>
        /// <returns></returns>
        public User GetUser()
        {
            User user;

            var serializer = new XmlSerializer(typeof(User), new XmlRootAttribute("User"));

            StreamReader reader = new StreamReader(_dataFilePath);
            using (reader)
            {
                user = (User)serializer.Deserialize(reader);
            }

            return user;
        }

        /// <summary>
        /// Save user to XML file
        /// </summary>
        /// <param name="user"></param>
        public void SaveUser(User user)
        {
            var serializer = new XmlSerializer(typeof(User), new XmlRootAttribute("User"));

            StreamWriter writer = new StreamWriter(_dataFilePath);
            using (writer)
            {
                serializer.Serialize(writer, user);
            }

        }
    }
}
