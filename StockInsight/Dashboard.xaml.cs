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
using StockInsight.Model;
using StockInsight.BAL;
using System.Timers;

namespace StockInsight
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        private Context context;
        private BusinessLayer bal;
        private string message;
        Timer ResetTimer = new Timer();
        private int refreshDisableTime = 30000;

        public Dashboard(Context _context, BusinessLayer _bal)
        {
            InitializeComponent();
            context = _context;
            bal = _bal;
            BindToDataGrid(context.StockIntraday);
            InitializeTimer(ResetTimer);
        }

        private void BindToDataGrid(List<Stock> stocks)
        {
            dataGrid_Dashboard.ItemsSource = stocks;
        }

        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            message = "";
            try
            {
                bal.ReadStockIntradayApiData(out message);
                InitializeTimer(ResetTimer);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (message == "" || message == null)
                {
                    BindToDataGrid(context.StockIntraday);
                }
            }
        }

        /// <summary>
        /// Start Timer and disable refresh button
        /// </summary>
        /// <param name="_timer"></param>
        private void InitializeTimer(Timer _timer)
        {
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Interval = refreshDisableTime;
            _timer.AutoReset = false;
            _timer.Start();

            btn_Refresh.IsEnabled = false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                btn_Refresh.IsEnabled = true;
            });
        }

        private void TextBox_Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if(textBox_Search.Text == "Search...")
            {
                textBox_Search.Text = "";
            }
        }

        private void TextBox_Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_Search.Text == "")
            {
                textBox_Search.Text = "Search...";
            }
        }

        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            string symbol = textBox_Search.Text.ToUpper();

            if (bal.ValidSymbol(symbol, out message)) {
                try
                {
                    bal.AddStockToWatchlist(symbol, out message);
                }
                catch (Exception)
                {
                    message = $"Unable to find stock symbol {symbol}.";
                }
                finally
                {
                    if(message == "" || message == null)
                    {
                        lbl_SearchError.Content = "";
                        BindToDataGrid(context.StockIntraday);
                    }
                    else
                    {
                        lbl_SearchError.Content = message;
                    }
                }
            }
            else
            {
                lbl_SearchError.Content = message;
            }
        }

        private void Btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
