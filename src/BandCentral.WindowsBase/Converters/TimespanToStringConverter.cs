using System;
using Windows.UI.Xaml.Data;

namespace BandCentral.WindowsBase.Converters
{
    public class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan span = (TimeSpan)value;
            if(span == TimeSpan.MinValue)
            {
                return "0 seconds";
            }
            string formatted = string.Format("{0}{1}{2}{3}",
            span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
            span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
            span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
            span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);
            if(formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);
            if(string.IsNullOrEmpty(formatted)) formatted = "0 seconds";
            return formatted;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            TimeSpan resultSpan;
            if(TimeSpan.TryParse(strValue, out resultSpan))
            {
                return resultSpan;
            }

            throw new Exception("Unable to convert string to date time");
        }
    }
}
