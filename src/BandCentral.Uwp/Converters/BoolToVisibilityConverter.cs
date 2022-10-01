using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool val;

            if (IsInverted)
            {
                val = !(bool) value;
            }
            else
            {
                val = (bool) value;
            }

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (IsInverted)
                return Visibility.Collapsed == (Visibility) value;
            else
                return Visibility.Visible == (Visibility) value;
        }
    }
}
