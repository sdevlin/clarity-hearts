using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Hearts.Models;
using System.Windows.Media;

namespace UI.Converters
{
    public class CardToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var card = (Card)value;
            return new Uri(string.Format("resources/images/cards/{0}_{1}.png",
                (int)card.Rank + 1, card.Suit));
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
