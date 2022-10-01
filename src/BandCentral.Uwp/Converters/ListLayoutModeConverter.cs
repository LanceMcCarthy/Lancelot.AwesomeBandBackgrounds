using BandCentral.Models.Enums;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class ListLayoutModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var condition = (ListLayoutMode)value == ListLayoutMode.Wrap;
            bool isInverse;
            if(parameter != null && bool.TryParse(parameter.ToString(), out isInverse))
            {
                condition = condition ^ isInverse;
            }

            return condition ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var condition = (Visibility)value == Visibility.Visible;
            bool isInverse;
            if(parameter != null && bool.TryParse(parameter.ToString(), out isInverse))
            {
                condition = condition ^ isInverse;
            }

            return condition ? ListLayoutMode.Wrap : ListLayoutMode.List;
        }
    }
}
