using System;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var span = (TimeSpan)value;
            if(span == TimeSpan.MinValue)
            {
                return "0 seconds";
            }

            string formatted = $"{(span.Duration().Days > 0 ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                               $"{(span.Duration().Hours > 0 ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                               $"{(span.Duration().Minutes > 0 ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                               $"{(span.Duration().Seconds > 0 ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}" : string.Empty)}";
            
            if(formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if(string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var strValue = value as string;

            if(TimeSpan.TryParse(strValue, out var resultSpan))
            {
                return resultSpan;
            }

            throw new Exception("Unable to convert string to date time");
        }
    }
}
