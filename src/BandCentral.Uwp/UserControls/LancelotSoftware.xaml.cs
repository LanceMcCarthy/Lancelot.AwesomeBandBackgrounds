using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class LancelotSoftware : UserControl
    {
        public static readonly DependencyProperty LogoFontSizeProperty = DependencyProperty.Register(
            nameof(LogoFontSize), typeof (double), typeof (LancelotSoftware), new PropertyMetadata(18));

        public double LogoFontSize
        {
            get => (double) GetValue(LogoFontSizeProperty);
            set => SetValue(LogoFontSizeProperty, value);
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
