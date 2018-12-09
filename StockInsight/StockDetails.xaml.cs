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
using StockInsight.Utilities;

namespace StockInsight
{
    /// <summary>
    /// Interaction logic for StockDetails.xaml
    /// </summary>
    public partial class StockDetails : Window
    {
        private Stock stock;
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        private LineSeries line { get; set; }
        private StockType CurrentGraph { get; set; } 

        public StockDetails(Stock _stock)
        {
            InitializeComponent();
            stock = _stock;
            SetUpWindow();
        }

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

        private void AddNewTitleToWindow(string company)
        {
            Details.Title = $"{company} - Details";
        }

        private void BindSymbolToTop(string symbol)
        {
            lbl_Symbol.Content = symbol;
        }

        private void BindCurrentPriceToTop(double close)
        {
            lbl_Price.Content = $"${close.ToString("F")}";
        }

        private void BindLineSeriesToCollection(LineSeries _line)
        {
            SeriesCollection.Clear();
            SeriesCollection.Add(_line);
            StockChartAxisX.Labels = Labels;

            DataContext = this;
        }

        private void InitializeLineGraphChart(List<MonthChart> month)
        {
            CreateNewLineSeries();
            BindMonthlyDataToGrid(month);
        }

        private void BindMonthlyDataToGrid(List<MonthChart> month)
        {
            List<string> prices = month.Select(d => d.close).ToList();
            CurrentGraph = StockType.MONTH;

            line.Stroke = GetLineColor(GetLastPrice(prices), GetFirstPrice(prices));
            line.Values = GetPrices(prices);
            Labels = BindMonthDatesToAxis(month);
            BindLineSeriesToCollection(line);
        }

        private void BindDailyDataToGrid(List<DayChart> day)
        {
            List<string> prices = day.Select(d => d.close).ToList();
            CurrentGraph = StockType.DAY;

            line.Stroke = GetLineColor(GetLastPrice(prices), GetFirstPrice(prices));
            line.Values = GetPrices(prices);
            Labels = BindDayTimesToAxis(day);
            BindLineSeriesToCollection(line);
        }

        private void CreateNewLineSeries()
        {
            line = new LineSeries
            {
                Title = stock.Symbol,
                Values = new ChartValues<double>(),
                LineSmoothness = 0,
                PointGeometrySize = 0,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            Labels = new[] { "" };
            YFormatter = value => value.ToString("C");
        }

        private ChartValues<double> GetPrices(List<string> prices)
        {
            ChartValues<double> values = new ChartValues<double>();

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

        private Brush GetLineColor(double open, double close)
        {
            if (open > close)
            {
                return line.Stroke = WindowStyles.RedSI;
            }

            return line.Stroke = WindowStyles.GreenSI;
        }

        private string[] BindMonthDatesToAxis(List<MonthChart> month)
        {
            List<string> labelList = new List<string>();

            foreach(var day in month.AsEnumerable().Reverse())
            {
                DateTime temp = DateTime.Parse(day.date);
                labelList.Add(temp.ToString("MM/dd"));
            }

            return labelList.ToArray();
        }

        private string[] BindDayTimesToAxis(List<DayChart> day)
        {
            List<string> labelList = new List<string>();

            foreach (var time in day.AsEnumerable().Reverse())
            {
                if (time.close != null)
                {
                    labelList.Add(DateTime.Parse(time.minute).ToString("hh:mm tt"));
                }
            }

            return labelList.ToArray();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_MonthChart_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentGraph == StockType.DAY)
            {
                BindMonthlyDataToGrid(stock.MonthCharts);
            }
        }

        private void Btn_DayChart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentGraph == StockType.MONTH)
            {
                BindDailyDataToGrid(stock.DayCharts);
            }
        }

        private double GetFirstPrice(List<string> prices)
        {
            double price = 0;

            try
            {
                price = prices.First(x => x != null).ConvertStringToDouble();
            }
            catch { }

            return price;
        }

        private double GetLastPrice(List<string> prices)
        {
            double price = 0;
            try
            {
                price = prices.Last(x => x != null).ConvertStringToDouble();
            }
            catch { }

            return price;
        }

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
            string volume = stock.MonthCharts.Last(x => x.volume != null).volume;

            CompanyValue.Content = company;
            CeoValue.Content = ceo;
            IndustryValue.Content = industry;
            WebsiteValue.Content = website;
            ExchangeValue.Content = exchange;

            PeValue.Content = peRatio;
            WkHighValue.Content = $"${Wk52High}";
            WkLowValue.Content = $"${Wk52Low}";
            VolumeValue.Content = String.Format("{0:n}", volume);
        }
    }
}
