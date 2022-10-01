using BandCentral.Models.Favorites;
using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class FavImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            var fav = (FlickrFav)value;

            if (string.IsNullOrEmpty(fav.LocalImageFilePath))
            {
                Debug.WriteLine($"FavImageSourceConverter - LocalImageFilePath NULL - Using Medium640Url: {fav.Photo.Medium640Url}");
                return fav.Photo.Medium640Url;
            }
            else
            {
                Debug.WriteLine($"FavImageSourceConverter - LocalImageFilePath: {fav.LocalImageFilePath}");
                return fav.LocalImageFilePath;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
