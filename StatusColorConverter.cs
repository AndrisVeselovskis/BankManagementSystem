using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BankManagementSystem
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? Brushes.Transparent : Brushes.Red;
            }

            if (value is decimal balance)
            {
                return balance < 0 ? Brushes.Red : Brushes.Transparent;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
