using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class EmptyCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int) value == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
