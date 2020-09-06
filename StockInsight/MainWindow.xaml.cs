using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StockInsight.Model;
using StockInsight.BAL;

namespace StockInsight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Context context;
        private BusinessLayer bal;
        private Error error;

        public MainWindow()
        {
            InitializeComponent();
            InstantiateFields();
        }

        /// <summary>
        /// Instantiate global fields
        /// </summary>
        private void InstantiateFields()
        {
            context = new Context();
            bal = new BusinessLayer(context);
        }

        /// <summary>
        /// Get user from local storage
        /// </summary>
        private void GetUser()
        {
            bal.GetOrCreateUser(out Error error);
        }

        /// <summary>
        /// Load in all of the required data for the dashboard window on its initial load
        /// </summary>
        private void LoadInData()
        {
            bal.ReadSavedWatchlist(out error);

            if (context.Watchlist.Any())
            {
                bal.GetAllQuoteData(out error);
            }
        }

        /// <summary>
        /// Window Content Rendered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StockInsight_ContentRendered(object sender, EventArgs e)
        {
            await Task.Run(() => GetUser());
            await Task.Run(() => LoadInData());

            Dashboard dashboard = new Dashboard(context, bal);
            this.Close();
            dashboard.ShowDialog();
        }
    }
}
