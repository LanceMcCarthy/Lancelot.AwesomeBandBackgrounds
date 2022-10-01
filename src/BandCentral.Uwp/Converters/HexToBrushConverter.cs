using BandCentral.Models.Helpers;
using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace BandCentral.Uwp.Converters
{
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //example incoming value: AA1133
            try
            {
                return Helpers.HexToBrush(value?.ToString());
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"HexToBrushConverter Exception: {ex}");
                return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
