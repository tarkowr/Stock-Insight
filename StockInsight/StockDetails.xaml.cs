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
using StockInsight.BAL;
using StockInsight.Utilities;

namespace StockInsight
{
    /// <summary>
    /// Interaction logic for StockDetails.xaml
    /// </summary>
    public partial class StockDetails : Window
    {
        private Stock stock;
        private BusinessLayer bal;
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        private LineSeries line { get; set; }
        private StockType CurrentGraph { get; set; } 

        public StockDetails(Stock _stock, BusinessLayer _bal)
        {
            InitializeComponent();
            stock = _stock;
            bal = _bal;
            SetUpWindow();
        }

        /// <summary>
        /// Initialize properties, create line graph, bind monthly data to graph
        /// </summary>
        private void SetUpWindow()
        {
            CurrentGraph = StockType.MONTH;
            SeriesCollection = new SeriesCollection();

            AddNewTitleToWindow(stock.CompanyName);
            BindSymbolToTop(stock.Symbol);
            BindCurrentPriceToTop(stock.Close);
            BindStatsToWindow();
            InitializeLineGraphChart(stock.MonthCharts);
        }

        /// <summary>
        /// Change Window Title
        /// </summary>
        /// <param name="company"></param>
        private void AddNewTitleToWindow(string company)
        {
            Details.Title = $"{company} - Details";
        }

        /// <summary>
        /// Display stock symbol at top of window
        /// </summary>
        /// <param name="symbol"></param>
        private void BindSymbolToTop(string symbol)
        {
            lbl_Symbol.Content = symbol;
        }

        /// <summary>
        /// Display price above graph
        /// </summary>
        /// <param name="close"></param>
        private void BindCurrentPriceToTop(double close)
        {
            lbl_Price.Content = close.FormatStockPrice();
        }

        /// <summary>
        /// Add Line Series to Series Collection
        /// </summary>
        /// <param name="_line"></param>
        private void BindLineSeriesToCollection(LineSeries _line)
        {
            SeriesCollection.Clear();
            SeriesCollection.Add(_line);
            StockChartAxisX.Labels = Labels;

            DataContext = this;
        }

        /// <summary>
        /// Create new Line Graph and populate it with monthly data
        /// </summary>
        /// <param name="month"></param>
        private void InitializeLineGraphChart(List<Chart> month)
        {
            CreateNewLineSeries();
            BindMonthlyDataToGraph(month);
        }

        /// <summary>
        /// Bind monthly data to line graph and update several fields
        /// </summary>
        /// <param name="month"></param>
        private void BindMonthlyDataToGraph(List<Chart> month)
        {
            var prices = month.Select(d => d.close).ToList();
            double firstPrice = bal.GetFirstPrice(prices);
            double lastPrice = bal.GetLastPrice(prices);

            CurrentGraph = StockType.MONTH;

            BindPercentage(bal.CalculateChangePercentage(lastPrice, firstPrice), bal.CalculateGainLoss(lastPrice, firstPrice));
            line.Stroke = bal.GetLineColor(lastPrice, firstPrice);
            line.Values = GetPrices(prices);
            Labels = BindMonthDatesToAxis(month);
            BindLineSeriesToCollection(line);
        }

        /// <summary>
        /// Bind daily data to graph and update several fields
        /// </summary>
        /// <param name="day"></param>
        private void BindDailyDataToGraph(List<DayChart> day)
        {
            var prices = day.Select(d => d.close).ToList();
            double firstPrice = bal.GetFirstPrice(prices);
            double lastPrice = bal.GetLastPrice(prices);

            CurrentGraph = StockType.DAY;

            BindPercentage(bal.CalculateChangePercentage(lastPrice, firstPrice), bal.CalculateGainLoss(lastPrice, firstPrice));
            line.Stroke = bal.GetLineColor(lastPrice, firstPrice);
            line.Values = GetPrices(prices);
            Labels = BindDayTimesToAxis(day);
            BindLineSeriesToCollection(line);
        }

        /// <summary>
        /// Instantiate new LineSeries with default values
        /// </summary>
        private void CreateNewLineSeries()
        {
            line = new LineSeries
            {
                Values = new ChartValues<double>(),
                LineSmoothness = 0,
                PointGeometrySize = 0,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            Labels = new[] { "" };
            YFormatter = value => value.ToString("C");
        }

        /// <summary>
        /// Return formatted and valid ChartValues
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        private ChartValues<double> GetPrices(List<string> prices)
        {
            var values = new ChartValues<double>();

            if(prices.Count > 0)
            {
                foreach (var price in prices.AsEnumerable().Reverse())
                {
                    if (price != null)
                    {
                        values.Add(price.ConvertStringToDouble());
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// Return an array of formatted and valid dates
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        private string[] BindMonthDatesToAxis(List<Chart> month)
        {
            List<string> labelList = new List<string>();

            foreach(var day in month.AsEnumerable().Reverse())
            {
                DateTime temp = DateTime.Parse(day.date);
                labelList.Add(temp.ToString("MM/dd"));
            }

            return labelList.ToArray();
        }

        /// <summary>
        /// Return an array of formatted and valid times
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private string[] BindDayTimesToAxis(List<DayChart> day)
        {
            var labelList = new List<string>();

            foreach (var time in day.AsEnumerable().Reverse())
            {
                if (time.close != null && time.minute != null)
                {
                    labelList.Add(DateTime.Parse(time.minute).ToString("hh:mm tt"));
                }
            }

            return labelList.ToArray();
        }
        
        /// <summary>
        /// Close Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 30 Day Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_MonthChart_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentGraph == StockType.DAY)
            {
                BindMonthlyDataToGraph(stock.MonthCharts);
            }
        }

        /// <summary>
        /// 1 Day Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_DayChart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentGraph == StockType.MONTH)
            {
                BindDailyDataToGraph(stock.DayCharts);
            }
        }

        /// <summary>
        /// Populate window stats section with stock stats
        /// </summary>
        private void BindStatsToWindow()
        {
            string company = stock.CompanyName;
            string ceo = stock.CompanyData.CEO;
            string industry = stock.CompanyData.industry;
            string website = stock.CompanyData.website;
            string exchange = stock.CompanyData.exchange;

            string peRatio = stock.QuoteData.peRatio;
            string Wk52High = stock.QuoteData.week52High;
            string Wk52Low = stock.QuoteData.week52Low;
            string volume = stock.MonthCharts.LastOrDefault(x => x.volume != null).volume;

            CompanyValue.Content = company;
            CeoValue.Content = ceo;
            IndustryValue.Content = industry;
            WebsiteValue.Content = website;
            ExchangeValue.Content = exchange;

            PeValue.Content = peRatio;

            if(Wk52High != null)
            {
                WkHighValue.Content = $"{String.Format("{0:C}", double.Parse(Wk52High))}";
            }
            if(Wk52Low != null)
            {
                WkLowValue.Content = $"{String.Format("{0:C}", double.Parse(Wk52Low))}";
            }
            if(volume != null)
            {
                VolumeValue.Content = String.Format("{0:n0}", long.Parse(volume));
            }
        }

        /// <summary>
        /// Bind percentage change and value change to stock stats section
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="change"></param>
        private void BindPercentage(double percentage, double change)
        {
            string symbol = "";
            string currency = String.Format("{0:C}", change);

            if (percentage > 0)
            {
                symbol = "+";
                currency = $" (+{currency})";
            }
            else
            {
                currency = $" (-{currency})";
            }

            PercentValue.Content = $"{symbol}{percentage}% {currency}";
        }
    }
}
