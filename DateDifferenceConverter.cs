using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BankManagementSystem
{
    public class DateDifferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString && DateTime.TryParseExact(dateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime termDate))
            {
                return (termDate - DateTime.Now).TotalDays < 30 ? Brushes.Red : Brushes.Transparent;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
