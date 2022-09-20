using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BandCentral.Uwp.Common;
using BandCentral.WindowsBase.Common;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class PaletteSelector : UserControl
    {
        public static readonly DependencyProperty PaletteColorsProperty = DependencyProperty.Register(
            "PaletteColors", typeof (List<string>), typeof (PaletteSelector), new PropertyMetadata(default(List<string>)));

        public List<string> PaletteColors
        {
            get { return (List<string>) GetValue(PaletteColorsProperty); }
            set { SetValue(PaletteColorsProperty, value); }
        }
        
        public static readonly DependencyProperty BandThemeProperty = DependencyProperty.Register(
            "BandTheme", typeof (BandColorTheme), typeof (PaletteSelector), new PropertyMetadata(default(BandColorTheme)));

        public BandColorTheme BandTheme
        {
            get { return (BandColorTheme) GetValue(BandThemeProperty); }
            set { SetValue(BandThemeProperty, value); }
        }

        public PaletteSelector()
        {
            this.InitializeComponent();
        }
    }
}
