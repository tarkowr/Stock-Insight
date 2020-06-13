using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.Model
{
    public class User
    {
        public string UserId { get; set; }
        public DateTime Created { get; set; }

        public User()
        {
            UserId = Guid.NewGuid().ToString();
            Created = DateTime.Now;
        }

    }
}
