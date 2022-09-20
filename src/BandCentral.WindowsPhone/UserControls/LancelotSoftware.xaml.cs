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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace BandCentral.UserControls
{
    public sealed partial class LancelotSoftware : UserControl
    {
        public static readonly DependencyProperty LogoFontSizeProperty = DependencyProperty.Register(
            "LogoFontSize", typeof (double), typeof (LancelotSoftware), new PropertyMetadata(18));

        public double LogoFontSize
        {
            get { return (double) GetValue(LogoFontSizeProperty); }
            set { SetValue(LogoFontSizeProperty, value); }
        }

        public LancelotSoftware()
        {
            this.InitializeComponent();
            this.Loaded += LancelotSoftware_Loaded;
        }

        void LancelotSoftware_Loaded(object sender, RoutedEventArgs e)
        {
            SpinStory.RepeatBehavior = RepeatBehavior.Forever;
            SpinStory.AutoReverse = true;
            this.SpinStory.Begin();
        }
    }
}
