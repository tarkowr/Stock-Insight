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
using System.Collections.ObjectModel;

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
        List<Stock> FilteredStocks;
        public static int refreshDisableTime = 10000;

        public Dashboard(Context _context, BusinessLayer _bal)
        {
            InitializeComponent();
            context = _context;
            bal = _bal;
            SetupWindow();
        }

        /// <summary>
        /// Setup several controls before the window displays
        /// </summary>
        private void SetupWindow()
        {
            BindToDataGrid(context.Stocks);
            InitializeTimer(ResetTimer);
        }

        /// <summary>
        /// Bind a list of stocks to the watchlist datagrid
        /// </summary>
        /// <param name="stocks"></param>
        private void BindToDataGrid(List<Stock> stocks)
        {
            ObservableCollection<Stock> observableCollection = new ObservableCollection<Stock>(stocks);
            dataGrid_Dashboard.ItemsSource = observableCollection;
        }

        private void Window_Dashboard_ContentRendered(object sender, EventArgs e)
        {
            DisplayGetStartedText();
        }

        /// <summary>
        /// Search Textbox OnFocus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if(textBox_Search.Text == "Search...")
            {
                textBox_Search.Text = "";
            }
        }

        /// <summary>
        /// Search Textbox OffFocus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_Search.Text == "")
            {
                textBox_Search.Text = "Search...";
                lbl_SearchError.Content = "";
            }
        }

        /// <summary>
        /// Search Textbox KeyUp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Search_KeyUp(object sender, KeyEventArgs e)
        {
            FilterBySearchText();
        }

        /// <summary>
        /// Add Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            string symbol = textBox_Search.Text.ToUpper();
            message = "";

            ChangeMouseIcon(MouseIcons.LOADING);

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
                    FilterBySearchText();
                    DisplayGetStartedText();

                    if (bal.IsEmpty(message))
                    {
                        lbl_SearchError.Content = "";
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

            ChangeMouseIcon(MouseIcons.DEFAULT);
        }

        /// <summary>
        /// Refresh Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            message = "";

            ChangeMouseIcon(MouseIcons.LOADING);

            try
            {
                //bal.GetAllStockDailyData(out message);
                bal.GetAllStockQuoteData(out message);
                InitializeTimer(ResetTimer);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                ChangeMouseIcon(MouseIcons.DEFAULT);
                HandleBindingWithFilter();
            }
        }

        private void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Dashboard.SelectedItems.Count == 1)
            {
                Stock stock = (Stock)dataGrid_Dashboard.SelectedItem;
                bal.RemoveStockFromWatchlist(stock.Symbol);
                HandleBindingWithFilter();
                DisplayGetStartedText();
            }
        }

        /// <summary>
        /// Details (View) Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Details_Click(object sender, RoutedEventArgs e)
        {
            message = "";

            if (dataGrid_Dashboard.SelectedItems.Count == 1)
            {
                Stock stock = (Stock)dataGrid_Dashboard.SelectedItem;

                bal.GetStockDailyData(stock.Symbol, out message);
                bal.GetStockQuoteData(stock.Symbol, out message);
                bal.GetStockMonthlyData(stock.Symbol, out message);

                if(bal.IsEmpty(message))
                {
                    StockDetails stockDetails = new StockDetails(bal.GetStockBySymbol(stock.Symbol, context.Stocks));
                    stockDetails.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Exit Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Window Closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Dashboard_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            message = "";

            bal.SaveWatchList(out message);
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

        /// <summary>
        /// Re-enable timer after timer is elapsed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                btn_Refresh.IsEnabled = true;
            });
        }

        /// <summary>
        /// Filter watchlist datagrid by search box text
        /// </summary>
        private void FilterBySearchText()
        {
            string input = textBox_Search.Text.ToString();

            FilteredStocks = new List<Stock>(context.Stocks);

            FilteredStocks = bal.ReturnFilteredStocks(FilteredStocks, input);

            if (bal.IsEmpty(input))
            {
                BindToDataGrid(context.Stocks);
            }
            else
            {
                BindToDataGrid(FilteredStocks);
            }
        }

        private void HandleBindingWithFilter()
        {
            string defaultText = "SEARCH...";
            string input = textBox_Search.Text.ToString();

            if (input.ToUpper() != defaultText)
            {
                FilterBySearchText();
            }
            else
            {
                BindToDataGrid(context.Stocks);
            }
        }

        /// <summary>
        /// Change the current mouse icon
        /// Build with help from:
        /// https://stackoverflow.com/questions/11021422/how-do-i-display-wait-cursor-during-a-wpf-applications-startup/26662085#26662085
        /// </summary>
        /// <param name="mouseIcon"></param>
        private void ChangeMouseIcon(MouseIcons mouseIcon)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (mouseIcon)
                {
                    case MouseIcons.LOADING:
                        Mouse.OverrideCursor = Cursors.Wait;
                        break;
                    default:
                        Mouse.OverrideCursor = null;
                        break;
                }
            });
        }

        private void DisplayGetStartedText()
        {
            if (bal.IsWatchlistEmpty())
            {
                lbl_GetStarted.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_GetStarted.Visibility = Visibility.Hidden;
            }
        }
    }
}
