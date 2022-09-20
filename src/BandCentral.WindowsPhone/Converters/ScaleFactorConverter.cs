using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace BandCentral.Converters
{
    public class ScaleFactorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var baseSize = (double) parameter;
            return baseSize * App.ViewModel.DisplayScaleFactor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
