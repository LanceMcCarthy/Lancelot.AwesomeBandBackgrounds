// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Common;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class PaletteSelector : UserControl
    {
        public static readonly DependencyProperty PaletteColorsProperty = DependencyProperty.Register(
            nameof(PaletteColors), typeof (List<string>), typeof (PaletteSelector), new PropertyMetadata(default(List<string>)));

        public List<string> PaletteColors
        {
            get => (List<string>) GetValue(PaletteColorsProperty);
            set => SetValue(PaletteColorsProperty, value);
        }
        
        public static readonly DependencyProperty BandThemeProperty = DependencyProperty.Register(
            nameof(BandTheme), typeof (BandColorTheme), typeof (PaletteSelector), new PropertyMetadata(default(BandColorTheme)));

        public BandColorTheme BandTheme
        {
            get => (BandColorTheme) GetValue(BandThemeProperty);
            set => SetValue(BandThemeProperty, value);
        }

        public PaletteSelector()
        {
            this.InitializeComponent();
        }
    }
}
