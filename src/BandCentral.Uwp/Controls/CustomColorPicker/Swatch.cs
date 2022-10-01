// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using BandCentral.Models.Helpers;

namespace BandCentral.Uwp.Controls.CustomColorPicker
{
    public class Swatch
    {
        public Swatch()
        {

        }

        public Swatch(string title, string[] thumbStrings, List<SwatchColor> colors)
        {
            Title = title;
            ThumbOne = Helpers.ColorToBrush(thumbStrings[0]);
            ThumbTwo = Helpers.ColorToBrush(thumbStrings[1]);
            ThumbThree = Helpers.ColorToBrush(thumbStrings[2]);
            ThumbFour = Helpers.ColorToBrush(thumbStrings[3]);
            SwatchColors = colors;
        }

        public string Title { get; set; }

        public List<SwatchColor> SwatchColors { get; set; }

        public SolidColorBrush ThumbOne { get; set; }
        public SolidColorBrush ThumbTwo { get; set; }
        public SolidColorBrush ThumbThree { get; set; }
        public SolidColorBrush ThumbFour { get; set; }
    }
}