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

        public StockDetails(Stock _stock)
        {
            InitializeComponent();
            stock = _stock;
            SetUpWindow();
        }

        private void SetUpWindow()
        {
            AddNewTitleToWindow(stock.CompanyName);
            BindSymbolToTop(stock.Symbol);
            BindCurrentPriceToTop(stock.Close);
            InitializeLineGraphChart(stock.MonthCharts);
            BindMonthlyPricesToGraph(stock.MonthCharts);
            BindMonthDatesToAxis(stock.MonthCharts);
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
            lbl_Price.Content = $"${close}";
        }

        private void InitializeLineGraphChart(List<MonthChart> month)
        {
            List<string> labelList = new List<string>();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = stock.Symbol,
                    Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#30c19d")),
                    Values = new ChartValues<double>(),
                    LineSmoothness = 0,
                    PointGeometrySize = 0,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                }
            };

            YFormatter = value => value.ToString("C");

            Labels = labelList.ToArray();

            DataContext = this;
        }

        private void BindMonthlyPricesToGraph(List<MonthChart> month)
        {
            SeriesCollection[0].Values.Clear();

            foreach (var day in month.AsEnumerable().Reverse())
            {
                SeriesCollection[0].Values.Add(day.close.ConvertStringToDouble());
            }

            DataContext = this;
        }

        private void BindMonthDatesToAxis(List<MonthChart> month)
        {
            List<string> labelList = new List<string>();
            Array.Clear(Labels, 0, Labels.Length);

            foreach(var day in month.AsEnumerable().Reverse())
            {
                DateTime temp = DateTime.Parse(day.date);
                labelList.Add(temp.ToString("MM/dd"));
            }

            Labels = labelList.ToArray();

            DataContext = this;
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
