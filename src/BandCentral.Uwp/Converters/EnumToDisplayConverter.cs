using BandCentral.Models.Attributes;
using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace BandCentral.Uwp.Converters
{
    public class EnumToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //credit for this converter approach goes to Jerry Nixon
            if (value == null) return "Null Value";
            var member = value.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == value.ToString());
            if (member == null) return "Null Member";
            var attribute = member.GetCustomAttribute(typeof (DisplayAttribute), false) as DisplayAttribute;
            if (attribute == null) return value.ToString();

            return attribute.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

