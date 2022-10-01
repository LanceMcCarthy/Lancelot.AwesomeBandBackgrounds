using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml.Media;
using FlickrNet;

namespace BandCentral.Models.Helpers
{
    public static partial class Helpers
    {
        public static SolidColorBrush ColorToBrush(string color)
        {
            color = color.Replace("#", "");
            if (color.Length == 6)
            {
                return new SolidColorBrush(ColorHelper.FromArgb(255,
                    byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
            }
            else
            {
                return null;
            }
        }

        public static SolidColorBrush HexToBrush(string hexValue)
        {
            byte R = System.Convert.ToByte(hexValue.Substring(0, 2), 16);
            byte G = System.Convert.ToByte(hexValue.Substring(2, 2), 16);
            byte B = System.Convert.ToByte(hexValue.Substring(4, 2), 16);

            return new SolidColorBrush(Color.FromArgb(255, R, G, B));
        }

        private static readonly DateTime UnixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        //credit Jerry Nixon
        public static List<T> GetEnumAsList<T>()
        {
            var array = Enum.GetValues(typeof(T));
            var list = new List<T>();
            foreach (var item in array)
            {
                list.Add((T) item);
            }
            return list;
        }

        //credit Jerry Nixon
        public static T GetEnumDefaultValue<T>()
        {
            var fields = (typeof(T).GetRuntimeFields());
            var defaultValue = fields.FirstOrDefault(x => x.GetCustomAttribute(typeof(DefaultValueAttribute)) != null);
            if (defaultValue == null) return default(T);

            return (T) Enum.Parse(typeof(T), defaultValue.Name);
        }

        public static string SafeSearchLevelToString(SafetyLevel level)
        {
            switch (level)
            {
                case SafetyLevel.None:
                    return "3";
                case SafetyLevel.Safe:
                    return "1";
                case SafetyLevel.Moderate:
                    return "2";
                case SafetyLevel.Restricted:
                    return "3";
                default:
                    return "1";
            }
        }

        public static string SortOrderToString(PhotoSearchSortOrder order)
        {
            switch (order)
            {
                case PhotoSearchSortOrder.DatePostedAscending:
                    return "date-posted-asc";
                case PhotoSearchSortOrder.DatePostedDescending:
                    return "date-posted-desc";
                case PhotoSearchSortOrder.DateTakenAscending:
                    return "date-taken-asc";
                case PhotoSearchSortOrder.DateTakenDescending:
                    return "date-taken-desc";
                case PhotoSearchSortOrder.InterestingnessAscending:
                    return "interestingness-asc";
                case PhotoSearchSortOrder.InterestingnessDescending:
                    return "interestingness-desc";
                case PhotoSearchSortOrder.Relevance:
                    return "relevance";
                default:
                    return "relevance";
                    return String.Empty;
            }
        }

        private static IEnumerable<Enum> GetFlags(Enum input)
        {
            var enumValues = Enum.GetValues(input.GetType()).Cast<Enum>().ToList();

            var selectedValues = enumValues
                .Where(@enum => (Convert.ToInt64(input) & Convert.ToInt64(@enum)) != 0)
                .ToList();

            return selectedValues.Where(value => !selectedValues.Any(@enum => (Convert.ToInt64(value) & Convert.ToInt64(@enum)) != 0 && Convert.ToInt64(value) > Convert.ToInt64(@enum)));
        }

        public static string PopularitySortOrderToString(PopularitySort sortOrder)
        {
            switch (sortOrder)
            {
                case PopularitySort.Comments:
                    return "comments";
                case PopularitySort.Favorites:
                    return "favorites";
                case PopularitySort.Views:
                    return "views";
                default:
                    return String.Empty;
            }
        }

        public static string DateToUnixTimestamp(DateTime date)
        {
            TimeSpan ts = date - UnixStartDate;
            return ts.TotalSeconds.ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static DateTime UnixTimestampToDate(string timestamp)
        {
            if (String.IsNullOrEmpty(timestamp)) return DateTime.MinValue;
            try
            {
                return UnixTimestampToDate(Int64.Parse(timestamp, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            catch (FormatException)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime UnixTimestampToDate(long timestamp)
        {
            return UnixStartDate.AddSeconds(timestamp);
        }
        
    }
}
