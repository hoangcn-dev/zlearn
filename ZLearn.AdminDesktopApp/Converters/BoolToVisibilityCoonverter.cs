using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ZLearn.AdminDesktopApp.Converters
{
    public class BoolToVisibilityCoonverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolVal && boolVal)
                return Visibility.Visible;
            return Visibility.Collapsed;  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis && vis == Visibility.Visible)
                return true;
            return false;
        }
    }
}
