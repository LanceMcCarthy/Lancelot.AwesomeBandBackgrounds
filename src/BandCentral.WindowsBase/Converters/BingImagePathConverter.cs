using System;
using Windows.UI.Xaml.Data;
using BandCentralBase.Common;

namespace BandCentral.WindowsBase.Converters
{
    public class BingImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var image = value as BingImage;

            if (image == null)
                return "";

            return $"http://www.bing.com{image.url}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
