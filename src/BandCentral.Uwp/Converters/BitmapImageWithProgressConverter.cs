using BandCentral.Models.Favorites;
using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Uwp.Converters
{
    public class BitmapImageWithProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = value as FlickrFav;
            if (item == null)
                return null;

            var downloadUrl = parameter != null ? parameter.ToString() : item.Photo.Medium640Url;
            
            var bmi = new BitmapImage(new Uri(downloadUrl));

            bmi.DownloadProgress += (sender, e) =>
            {
                item.DownloadProgress = e.Progress.ToString();
            };

            bmi.ImageFailed += (sender, e) =>
            {
                item.DownloadProgress = e.ErrorMessage;
            };

            return bmi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
