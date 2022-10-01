using BandCentral.Models.Bing;
using System;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class BingImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BingImage image)
                return $"http://www.bing.com{image.url}";

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
