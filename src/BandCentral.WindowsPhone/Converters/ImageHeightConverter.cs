using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace BandCentral.Converters
{
    public class ImageHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var requestedHeight = (double) parameter;
            var scaledHeight = requestedHeight*App.ViewModel.DisplayScaleFactor;

            return Math.Floor(0.3290 * scaledHeight);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
