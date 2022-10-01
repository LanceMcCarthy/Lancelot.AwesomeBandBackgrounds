using System;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class PhotoSizeConverter : IValueConverter
    {
        public string WidthOrHeight { get; set; } = "Width";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (WidthOrHeight.ToLowerInvariant())
            {
                case "width":
                    return App.ViewModel.ListItemSize.Width;
                case "height":
                    return App.ViewModel.ListItemSize.Height;
                default:
                    throw new ArgumentOutOfRangeException(WidthOrHeight,"Value needs to be either Width or Height as a string");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
