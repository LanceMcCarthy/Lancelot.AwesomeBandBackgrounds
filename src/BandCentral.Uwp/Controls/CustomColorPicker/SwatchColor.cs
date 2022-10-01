// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using Windows.UI.Xaml.Media;
using BandCentral.Models.Helpers;

namespace BandCentral.Uwp.Controls.CustomColorPicker
{
    public class SwatchColor
    {
        public SwatchColor()
        {

        }

        public SwatchColor(string[] colorList)
        {
            One = Helpers.ColorToBrush(colorList[0]);
            Two = Helpers.ColorToBrush(colorList[1]);
            Three = Helpers.ColorToBrush(colorList[2]);
            Four = Helpers.ColorToBrush(colorList[3]);
            Five = Helpers.ColorToBrush(colorList[4]);
            Six = Helpers.ColorToBrush(colorList[5]);
        }

        public SolidColorBrush One { get; set; }
        public SolidColorBrush Two { get; set; }
        public SolidColorBrush Three { get; set; }
        public SolidColorBrush Four { get; set; }
        public SolidColorBrush Five { get; set; }
        public SolidColorBrush Six { get; set; }
    }
}