using System;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    internal class FileNameToFilePathConverter : IValueConverter
    {
        public bool IsRoamingFolder { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "";

            return IsRoamingFolder ? $"ms-appdata:///roaming/{(string)value}" : $"ms-appdata:///local/{(string)value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
