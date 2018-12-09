using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StockInsight.Utilities
{
    public static class WindowStyles
    {
        public static Brush GreenSI = (SolidColorBrush) new BrushConverter().ConvertFrom("#39DBB2");
        public static Brush RedSI = (SolidColorBrush)new BrushConverter().ConvertFrom("#db5939");
    }
}
