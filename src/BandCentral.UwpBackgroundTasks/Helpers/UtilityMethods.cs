using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace BandCentral.UwpBackgroundTasks.Helpers
{
    public static class UtilityMethods
    {
        private const string PhotoUrlFormat = "https://farm{0}.staticflickr.com/{1}/{2}_{3}{4}.{5}";
        private static readonly DateTime unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTimeOffset UnixTimestampToDate(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
                return DateTime.MinValue;
            try
            {
                return UtilityMethods.UnixTimestampToDate(long.Parse(timestamp, NumberStyles.Any, (IFormatProvider)NumberFormatInfo.InvariantInfo));
            }
            catch (FormatException ex)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime UnixTimestampToDate(long timestamp) => UtilityMethods.unixStartDate.AddSeconds((double)timestamp);
        
        private static IEnumerable<Enum> GetValues(Enum enumeration)
        {
            List<Enum> values = new List<Enum>();
            foreach (FieldInfo runtimeField in enumeration.GetType().GetRuntimeFields())
                values.Add(runtimeField.GetValue((object)enumeration) as Enum);
            return (IEnumerable<Enum>)values;
        }
        
        internal static string UrlFormat(Photo p, string size, string extension) => size == "_o" || size == "original" ? UtilityMethods.UrlFormat(p.Farm, p.Server, p.PhotoId, p.OriginalSecret, size, extension) : UtilityMethods.UrlFormat(p.Farm, p.Server, p.PhotoId, p.Secret, size, extension);

        internal static string UrlFormat(
            string farm,
            string server,
            string photoid,
            string secret,
            string size,
            string extension)
        {
            switch (size)
            {
                case "square":
                    size = "_s";
                    break;
                case "thumbnail":
                    size = "_t";
                    break;
                case "small":
                    size = "_m";
                    break;
                case "medium":
                    size = string.Empty;
                    break;
                case "large":
                    size = "_b";
                    break;
                case "original":
                    size = "_o";
                    break;
            }
            return UtilityMethods.UrlFormat("https://farm{0}.staticflickr.com/{1}/{2}_{3}{4}.{5}", (object)farm, (object)server, (object)photoid, (object)secret, (object)size, (object)extension);
        }

        private static string UrlFormat(string format, params object[] parameters) => string.Format((IFormatProvider)CultureInfo.InvariantCulture, format, parameters);

        public static DateTimeOffset ParseDateWithGranularity(string date)
        {
            DateTime dateWithGranularity = DateTime.MinValue;
            if (string.IsNullOrEmpty(date) || date == "0000-00-00 00:00:00")
                return dateWithGranularity;
            if (date.EndsWith("-00-01 00:00:00"))
            {
                dateWithGranularity = new DateTime(int.Parse(date.Substring(0, 4), (IFormatProvider)NumberFormatInfo.InvariantInfo), 1, 1);
                return dateWithGranularity;
            }
            string format = "yyyy-MM-dd HH:mm:ss";
            try
            {
                dateWithGranularity = DateTime.ParseExact(date, format, (IFormatProvider)DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch (FormatException ex)
            {
            }
            return dateWithGranularity;
        }
        
    }
}