using System;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0;

            return double.TryParse((string) value, out var parsedValue) 
                ? parsedValue 
                : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
