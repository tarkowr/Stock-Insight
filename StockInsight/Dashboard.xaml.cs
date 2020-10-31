using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        private Error error;
        private Reporter logger = new Reporter();
        private Timer ResetTimer = new Timer();
        private List<Stock> FilteredStocks;
        private bool dataLock = false;

        public static int fetchInterval = 60000;
        public static string searchText = "Search...";

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
            dataGrid_Dashboard.ItemsSource = new ObservableCollection<Stock>(stocks);
        }

        /// <summary>
        /// Window Content Rendered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            if(textBox_Search.Text == searchText)
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
                textBox_Search.Text = searchText;
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
            FilterBySearchTextAndBind();
        }

        /// <summary>
        /// Add Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (dataLock) return;

            string symbol = textBox_Search.Text.ToUpper();
            message = "";

            if (!bal.ValidSymbol(symbol, out message))
            {
                lbl_SearchError.Content = message;
                return;
            }

            await RunTaskAsyncWithLock(() => bal.AddStockToWatchlist(symbol, out message));

            FilterBySearchTextAndBind();
            DisplayGetStartedText();
            lbl_SearchError.Content = message;
        }

        /// <summary>
        /// Remove Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (dataLock) return;

            if (dataGrid_Dashboard.SelectedItems.Count == 1)
            {
                var stock = (Stock)dataGrid_Dashboard.SelectedItem;

                await RunTaskAsyncWithLock(() => bal.RemoveStockFromWatchlist(stock.Symbol));

                FilterBySearchTextAndBind();
                DisplayGetStartedText();
            }
        }

        /// <summary>
        /// Details (View) Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Details_Click(object sender, RoutedEventArgs e)
        {
            if (dataLock) return;

            if (dataGrid_Dashboard.SelectedItems.Count == 1)
            {
                var stock = (Stock)dataGrid_Dashboard.SelectedItem;

                if (stock == null || !bal.DoesStockExist(stock.Symbol, context.Stocks)) return;

                await RunTaskAsyncWithLock(() => bal.GetStockDetailData(stock, out error));

                if (error.Equals(Error.NONE))
                {
                    var selectedStock = bal.GetStockBySymbol(stock.Symbol, context.Stocks);

                    if (selectedStock != null)
                    {
                        var stockDetails = new StockDetails(selectedStock, bal);
                        stockDetails.ShowDialog();
                    }
                }
            }
        }

        /// <summary>
        /// Run a time consuming task async with a lock on data
        /// </summary>
        /// <param name="action"></param>
        private async Task<bool> RunTaskAsyncWithLock(Action action)
        {
            ChangeMouseIcon(MouseIcons.LOADING);
            dataLock = true;
            await Task.Run(action);
            dataLock = false;
            ChangeMouseIcon(MouseIcons.DEFAULT);

            return true;
        }

        /// <summary>
        /// Get live stock data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FetchStockData(out Error error)
        {
            if (!dataLock)
            {
                logger.info("Fetching Latest Stock Prices");
                bal.GetAllQuoteData(out error);
            }
            else
            {
                logger.info("Skipping Stock Data Fetch because of Data Lock");
                error = Error.OTHER;
            }
        }

        /// <summary>
        /// Start Timer and disable refresh button
        /// </summary>
        /// <param name="_timer"></param>
        private void InitializeTimer(Timer _timer)
        {
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Interval = fetchInterval;
            _timer.AutoReset = true;
            _timer.Start();
        }

        /// <summary>
        /// Get live stock data on interval if stocks are trading
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (bal.AreStocksCurrentlyTrading())
            {
                await Task.Run(() => FetchStockData(out error));

                if (!error.Equals(Error.NONE))
                {
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    FilterBySearchTextAndBind();
                });
            }
        }

        /// <summary>
        /// Filter watchlist datagrid by search box text
        /// </summary>
        private void FilterBySearchTextAndBind()    
        {
            string input = textBox_Search.Text.ToString();

            if (bal.IsEmpty(input) || input.ToUpper() == searchText.ToUpper())
            {
                BindToDataGrid(context.Stocks);
                return;
            }

            FilteredStocks = new List<Stock>(context.Stocks);
            FilteredStocks = bal.ReturnFilteredStocks(FilteredStocks, input);

            BindToDataGrid(FilteredStocks);
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

        /// <summary>
        /// Display get started text if Watchlist is empty
        /// </summary>
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
