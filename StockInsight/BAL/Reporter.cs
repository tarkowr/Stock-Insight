using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInsight.BAL
{
    public class Reporter
    {
        public void info(string message)
        {
            Console.WriteLine($"INFO: {message}");
        }

        public void warning(string message)
        {
            Console.WriteLine($"WARNING: {message}");
        }

        public void error(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }
    }
}
