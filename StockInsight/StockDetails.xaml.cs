using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using StockInsight.Model;

namespace StockInsight
{
    /// <summary>
    /// Interaction logic for StockDetails.xaml
    /// </summary>
    public partial class StockDetails : Window
    {
        Context context;

        public StockDetails(Context _context)
        {
            InitializeComponent();
            context = _context;
        }
    }
}
