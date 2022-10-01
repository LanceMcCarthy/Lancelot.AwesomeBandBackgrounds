// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/UwpProjects for custom controls
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds for original app

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace BandCentral.Uwp.Controls
{
    public sealed partial class BandBusyIndicator : UserControl
    {
        private bool isLoaded;

        public BandBusyIndicator()
        {
            this.InitializeComponent();
            Loaded += BandBusyIndicator_Loaded;
        }

        public static readonly DependencyProperty DisplayMessageProperty = DependencyProperty.Register(
            nameof(DisplayMessage), typeof (string), typeof (BandBusyIndicator), new PropertyMetadata(""));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(BandBusyIndicator), new PropertyMetadata(false, (o, args) =>
            {
                ((BandBusyIndicator) o).IsActivePropertyChanged(args);
            }));

        public static readonly DependencyProperty IsReversedProperty = DependencyProperty.Register(
            nameof(IsReversed), typeof(bool), typeof(BandBusyIndicator), new PropertyMetadata(false));

        public string DisplayMessage
        {
            get => (string) GetValue(DisplayMessageProperty);
            set => SetValue(DisplayMessageProperty, value);
        }

        public bool IsActive
        {
            get => (bool) GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public bool IsReversed
        {
            get => (bool) GetValue(IsReversedProperty);
            set => SetValue(IsReversedProperty, value);
        }

        private void IsActivePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!isLoaded)
                return;

            if ((bool) e.NewValue)
            {
                TopDownBusyStory.RepeatBehavior = RepeatBehavior.Forever;

                this.Visibility = Visibility.Visible;

                if (IsReversed)
                    TopDownBusyStory.AutoReverse = true;

                TopDownBusyStory.Begin();
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                TopDownBusyStory.Stop();
            }
        }
        
        private void BandBusyIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            this.isLoaded = true;
            IsActive = IsActive;
            
        }
    }
}
