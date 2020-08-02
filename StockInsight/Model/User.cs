using System;

namespace StockInsight.Model
{
    public class User
    {
        public string UserId { get; set; }
        public DateTime Created { get; set; }

        public void Create()
        {
            UserId = Guid.NewGuid().ToString();
            Created = DateTime.Now;
        }

    }
}
